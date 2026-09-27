using UnityEngine;

namespace Infra2DAction
{
    public enum ItemType { Consumable, Equipable, QuestKey, Material }

    [CreateAssetMenu(fileName = "ItemData_", menuName = "Infraestructura2DAction/Content/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identification")]
        public string ItemID;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Type")]
        public ItemType Type;

        [Header("Shop")]
        public int BasePrice = 10;

        [Header("Equipable Effects (solo si Type = Equipable)")]
        [Tooltip("Variable que modifica: AttackDamage, MaxHP, HealRecoveryTime o MaxResource.")]
        public string EffectTargetVariable;
        public float EffectAmount;
    }
}
