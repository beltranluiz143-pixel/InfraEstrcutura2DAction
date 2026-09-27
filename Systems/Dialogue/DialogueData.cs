using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "DialogueData_", menuName = "Infraestructura2DAction/Content/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public enum DecisionType
        {
            None, AddItem, RemoveItem, GiveMoney, RouteEffect, NpcHelped
        }

        [Serializable]
        public class DialogueChoice
        {
            public string Text;
            public DecisionType Decision = DecisionType.None;

            [Tooltip("Cantidad de dinero (GiveMoney: positivo da, negativo cobra) o de items (Add/RemoveItem, minimo 1).")]
            public float DecisionAmount = 1f;

            [Tooltip("ID del item (Add/RemoveItem) o del NPC (NpcHelped).")]
            public string DecisionTargetID;

            [Tooltip("Solo para RouteEffect: accion de ruta que se dispara al elegir esta opcion.")]
            public RouteActionData RouteAction;

            [Tooltip("Indice de la linea a la que salta tras elegir. -1 = continua secuencialmente.")]
            public int NextLineIndex = -1;
        }

        [Serializable]
        public class DialogueLine
        {
            public string CharacterID;

            [TextArea(2, 5)]
            public string Text;

            public Sprite Portrait;

            [Tooltip("Condicion evaluada antes de mostrar esta linea.")]
            public FlagCondition Condition;

            [Tooltip("Linea alternativa si la Condition no se cumple.")]
            public DialogueLine AlternativeLine;

            [Tooltip("Si tiene opciones, el jugador elige. Si esta vacio, avanza automaticamente.")]
            public List<DialogueChoice> Choices = new List<DialogueChoice>();
        }

        [Header("Identification")]
        public string DialogueID;

        [Header("Panel")]
        [Tooltip("PanelID del panel de dialogo a usar. Vacio = panel por defecto del DialogueSystem.")]
        public string DialoguePanelID = "";

        [Header("Lines")]
        public List<DialogueLine> Lines = new List<DialogueLine>();
    }
}
