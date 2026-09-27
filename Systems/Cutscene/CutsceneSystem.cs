using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public class CutsceneSystem : MonoBehaviour
    {
        private TransitionController _transitionController;

        private bool _isRunning;
        private bool _waitingForDialogue;

        private readonly List<CutsceneCharacterReference> _sceneCharacters = new List<CutsceneCharacterReference>();
        private readonly List<Coroutine> _parallelCoroutines = new List<Coroutine>();
        private readonly List<Collider2D> _openCameraZones = new List<Collider2D>();

        public void Initialize(TransitionController transitionController)
        {
            _transitionController = transitionController;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CutsceneStartRequestEvent>(OnCutsceneStartRequested);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Subscribe<CutsceneCharacterRegisteredEvent>(OnCharacterRegistered);
            EventBus.Subscribe<CutsceneCharacterUnregisteredEvent>(OnCharacterUnregistered);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CutsceneStartRequestEvent>(OnCutsceneStartRequested);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Unsubscribe<CutsceneCharacterRegisteredEvent>(OnCharacterRegistered);
            EventBus.Unsubscribe<CutsceneCharacterUnregisteredEvent>(OnCharacterUnregistered);
        }

        private void OnCharacterRegistered(CutsceneCharacterRegisteredEvent e)
        {
            if (!_sceneCharacters.Contains(e.Character))
                _sceneCharacters.Add(e.Character);
        }

        private void OnCharacterUnregistered(CutsceneCharacterUnregisteredEvent e)
        {
            _sceneCharacters.Remove(e.Character);
        }

        private void OnCutsceneStartRequested(CutsceneStartRequestEvent e)
        {
            if (_isRunning || e.Data == null || e.Data.Commands.Count == 0) return;
            StartCoroutine(RunCutscene(e.Data));
        }

        private IEnumerator RunCutscene(CutsceneData data)
        {
            _isRunning = true;

            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Cutscene, "CutsceneSystem"));
            EventBus.Raise(new CutsceneAnimationOverrideEvent(true));
            EventBus.Raise(new CutsceneStartedEvent());

            _parallelCoroutines.Clear();
            _openCameraZones.Clear();

            foreach (CutsceneData.CutsceneCommand command in data.Commands)
            {
                if (command.WaitForCompletion)
                {
                    yield return ExecuteCommand(command);
                }
                else
                {
                    Coroutine parallel = StartCoroutine(ExecuteCommand(command));
                    _parallelCoroutines.Add(parallel);
                }
            }

            StopAllParallelCoroutines();
            CloseAllCameraZones();

            EventBus.Raise(new CutsceneAnimationOverrideEvent(false));
            EventBus.Raise(new GameStateChangeRequestEvent(GameStateType.Gameplay, "CutsceneSystem"));
            EventBus.Raise(new CutsceneEndedEvent());

            _isRunning = false;
        }

        private void StopAllParallelCoroutines()
        {
            foreach (Coroutine c in _parallelCoroutines)
                if (c != null) StopCoroutine(c);

            _parallelCoroutines.Clear();
        }

        private void CloseAllCameraZones()
        {
            foreach (Collider2D bounds in _openCameraZones)
                EventBus.Raise(new CameraZoneExitedEvent(bounds, "CutsceneSystem"));

            _openCameraZones.Clear();
        }

        private IEnumerator ExecuteCommand(CutsceneData.CutsceneCommand command)
        {
            switch (command.Type)
            {
                case CutsceneData.CommandType.MoveCharacter:
                    yield return ExecuteMoveCharacter(command);
                    break;

                case CutsceneData.CommandType.PlayAnimation:
                    EventBus.Raise(new CutsceneForceAnimationEvent(command.TargetCharacterID, command.AnimationTrigger));
                    break;

                case CutsceneData.CommandType.ShowDialogue:
                    yield return ExecuteShowDialogue(command);
                    break;

                case CutsceneData.CommandType.CameraFocus:
                    ExecuteCameraFocus(command);
                    break;

                case CutsceneData.CommandType.Wait:
                    yield return new WaitForSeconds(command.WaitDuration);
                    break;

                case CutsceneData.CommandType.FadeScreen:
                    yield return ExecuteFadeScreen(command);
                    break;

                case CutsceneData.CommandType.PlayMusic:
                    EventBus.Raise(new MusicPlayRequestEvent(command.AudioOrVFXID, "CutsceneSystem"));
                    break;

                case CutsceneData.CommandType.PlaySFX:
                    EventBus.Raise(new SFXPlayRequestEvent(command.AudioOrVFXID, transform.position, "CutsceneSystem"));
                    break;

                case CutsceneData.CommandType.PlayVFX:
                    EventBus.Raise(new PoolSpawnRequestEvent(command.AudioOrVFXID, transform.position, default, "CutsceneSystem"));
                    break;
            }
        }

        private IEnumerator ExecuteMoveCharacter(CutsceneData.CutsceneCommand command)
        {
            Transform target = FindCharacterTransform(command.TargetCharacterID);
            if (target == null) yield break;

            Vector3 startPos = target.position;
            Vector3 endPos = new Vector3(command.TargetPosition.x, command.TargetPosition.y, startPos.z);

            float elapsed = 0f;
            while (elapsed < command.MoveDuration)
            {
                elapsed += Time.deltaTime;
                target.position = Vector3.Lerp(startPos, endPos, elapsed / command.MoveDuration);
                yield return null;
            }

            target.position = endPos;
        }

        private IEnumerator ExecuteShowDialogue(CutsceneData.CutsceneCommand command)
        {
            if (command.DialogueReference == null) yield break;

            _waitingForDialogue = true;
            EventBus.Raise(new DialogueStartRequestEvent(command.DialogueReference, "CutsceneSystem"));

            while (_waitingForDialogue)
                yield return null;
        }

        private void OnDialogueEnded(DialogueEndedEvent e) => _waitingForDialogue = false;

        private void ExecuteCameraFocus(CutsceneData.CutsceneCommand command)
        {
            if (command.CameraBounds == null) return;

            EventBus.Raise(new CameraZoneEnteredEvent(
                command.CameraBounds, command.CameraDataOverride, command.CameraPriority, "CutsceneSystem"));

            _openCameraZones.Add(command.CameraBounds);
        }

        private IEnumerator ExecuteFadeScreen(CutsceneData.CutsceneCommand command)
        {
            if (_transitionController == null) yield break;

            if (command.FadeOut)
                yield return _transitionController.FadeOut(command.FadeColor);
            else
                yield return _transitionController.FadeIn();
        }

        private Transform FindCharacterTransform(string characterID)
        {
            foreach (CutsceneCharacterReference character in _sceneCharacters)
                if (character != null && character.CharacterID == characterID)
                    return character.CharacterTransform;

            return null;
        }
    }
}
