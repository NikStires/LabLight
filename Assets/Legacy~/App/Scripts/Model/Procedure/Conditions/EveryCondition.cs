using Sirenix.OdinInspector;

/// <summary>
/// Condition to show a visualization for every detection
/// </summary>
[LabelText("Every Condition")]
public class EveryCondition : Condition
{
    /// <summary>
    /// Label filter
    /// </summary>
    public string filter = "";

    public EveryCondition()
    {
        conditionType = ConditionType.Every;
    }

    public override bool IsSpecific()
    {
        return false;
    }

    public override bool IsTargeted()
    {
        return !string.IsNullOrEmpty(filter);
    }

    public override string[] Targets()
    {
        return new string[] { filter };
    }
}
