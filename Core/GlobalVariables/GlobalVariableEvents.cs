namespace Infra2DAction
{
    public class GlobalFlagChangedEvent : BaseEvent
    {
        public string FlagKey { get; private set; }
        public bool Value { get; private set; }

        public GlobalFlagChangedEvent(string flagKey, bool value, string sourceID = "GlobalVariablesSystem")
            : base(sourceID)
        {
            FlagKey = flagKey;
            Value = value;
        }
    }

    public class GlobalVariableChangedEvent : BaseEvent
    {
        public string VariableKey { get; private set; }
        public float Value { get; private set; }

        public GlobalVariableChangedEvent(string variableKey, float value, string sourceID = "GlobalVariablesSystem")
            : base(sourceID)
        {
            VariableKey = variableKey;
            Value = value;
        }
    }

    public class FlagQueryEvent : BaseEvent
    {
        public string Key { get; private set; }
        public bool Handled { get; private set; }
        public bool Result { get; private set; }

        public FlagQueryEvent(string key, string sourceID = "Unknown")
            : base(sourceID)
        {
            Key = key;
        }

        public void SetResult(bool value)
        {
            Result = value;
            Handled = true;
        }
    }

    public class FlagSetRequestEvent : BaseEvent
    {
        public string Key { get; private set; }
        public bool Value { get; private set; }

        public FlagSetRequestEvent(string key, bool value, string sourceID = "Unknown")
            : base(sourceID)
        {
            Key = key;
            Value = value;
        }
    }

    public class VariableQueryEvent : BaseEvent
    {
        public string Key { get; private set; }
        public bool Handled { get; private set; }
        public float Result { get; private set; }

        public VariableQueryEvent(string key, string sourceID = "Unknown")
            : base(sourceID)
        {
            Key = key;
        }

        public void SetResult(float value)
        {
            Result = value;
            Handled = true;
        }
    }

    public class ConditionQueryEvent : BaseEvent
    {
        public FlagCondition Condition { get; private set; }
        public bool Handled { get; private set; }
        public bool Result { get; private set; }

        public ConditionQueryEvent(FlagCondition condition, string sourceID = "Unknown")
            : base(sourceID)
        {
            Condition = condition;
        }

        public void SetResult(bool value)
        {
            Result = value;
            Handled = true;
        }
    }
}
