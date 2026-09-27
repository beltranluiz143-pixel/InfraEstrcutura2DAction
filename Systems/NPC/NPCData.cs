using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "NPCData_", menuName = "Infraestructura2DAction/Content/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Serializable]
        public class ConditionalDialogue
        {
            [Tooltip("Condicion que debe cumplirse para usar este dialogo. " +
                     "Si esta vacio, se evalua siempre como verdadero (fallback).")]
            public FlagCondition Condition;
            public DialogueData Dialogue;

            [Tooltip("Prioridad: gana el dialogo valido con mayor numero.")]
            public int Priority = 0;
        }

        [Header("Identification")]
        public string NPCID;

        [Header("Resource Effect")]
        [Tooltip("Al interactuar: positivo entrega recurso al jugador, negativo lo vacia, 0 no hace nada.")]
        public float ResourceEffect = 0f;

        [Header("Linked Quest (opcional)")]
        [Tooltip("Quest que se inicia al hablar con este NPC.")]
        public QuestData LinkedQuest;

        [Header("Dialogue States")]
        public List<ConditionalDialogue> DialogueStates = new List<ConditionalDialogue>();

        [Header("Fallback")]
        public DialogueData FallbackDialogue;
    }
}
