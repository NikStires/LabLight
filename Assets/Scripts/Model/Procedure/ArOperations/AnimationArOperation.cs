
/// <summary>
/// Operation to play a certain animation on an ArView
/// </summary>
public class AnimationArOperation : ArOperation
{
    /// <summary>
    /// Name of the animation to play
    /// </summary>
    public string AnimationName;

    public AnimationArOperation()
    {
        arOperationType = ArOperationType.Animation;
    }

    //public override void Apply(ArElementViewController arView)
    //{
    //    ((ModelElementViewController)arView).PlayAnimation(AnimationName);
    //}

    public override string ListElementLabelName()
    {
        return "Animation Operation";
    }
}

