using UnityEngine;

namespace Infra2DAction
{
    public enum MinigameRewardType { Heal, GiveCurrency, DamageBoss }

    [CreateAssetMenu(fileName = "MinigameData_", menuName = "Infraestructura2DAction/Content/Minigame Data")]
    public class MinigameData : ScriptableObject
    {
        [Header("Identification")]
        public string MinigameID;

        [Header("Prefab")]
        [Tooltip("Prefab con su propio Canvas a pantalla completa y un componente que implemente IMinigame.")]
        public GameObject Prefab;

        [Header("Success Reward (elige UNA)")]
        public MinigameRewardType RewardType = MinigameRewardType.Heal;
        public int RewardAmount = 1;

        [Header("Failure Penalty")]
        public int FailureDamage = 1;

        [Header("Difficulty (generico, cada minijuego interpreta estos numeros)")]
        public float[] DifficultyParams;
    }
}
