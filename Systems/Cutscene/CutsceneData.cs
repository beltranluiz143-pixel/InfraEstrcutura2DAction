using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "CutsceneData_", menuName = "Infraestructura2DAction/Content/Cutscene Data")]
    public class CutsceneData : ScriptableObject
    {
        public enum CommandType
        {
            MoveCharacter, PlayAnimation, ShowDialogue,
            CameraFocus, Wait, FadeScreen,
            PlayMusic, PlaySFX, PlayVFX
        }

        [Serializable]
        public class CutsceneCommand
        {
            public CommandType Type;

            [Header("MoveCharacter")]
            public string TargetCharacterID;
            public Vector2 TargetPosition;
            public float MoveDuration = 1f;

            [Header("PlayAnimation")]
            public string AnimationTrigger;

            [Header("ShowDialogue")]
            public DialogueData DialogueReference;

            [Header("CameraFocus")]
            public Collider2D CameraBounds;
            public CameraData CameraDataOverride;
            public int CameraPriority = 5;

            [Header("Wait")]
            public float WaitDuration = 1f;

            [Header("FadeScreen")]
            public Color FadeColor = Color.black;
            public bool FadeOut = true;

            [Header("Audio/VFX")]
            public string AudioOrVFXID;

            [Header("Flow")]
            [Tooltip("Si false, el siguiente comando se ejecuta en paralelo sin esperar a este.")]
            public bool WaitForCompletion = true;
        }

        [Header("Identification")]
        public string CutsceneID;

        [Header("Commands (ordenados)")]
        public List<CutsceneCommand> Commands = new List<CutsceneCommand>();
    }
}
