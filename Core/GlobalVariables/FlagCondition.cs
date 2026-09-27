using UnityEngine;

namespace Infra2DAction
{
    [CreateAssetMenu(fileName = "Condition_", menuName = "Infraestructura2DAction/Flags/Flag Condition")]
    public class FlagCondition : ScriptableObject
    {
        public enum ConditionType
        {
            FlagIsTrue,
            FlagIsFalse,
            VariableGreaterThan,
            VariableLessThan,
            VariableEquals
        }

        public ConditionType Type;
        public string Key;
        public float CompareValue;
    }
}
