namespace fms.Effect;

public class TrickshotBounceCount : EffectBase
{
    public required int BounceCount { get; init; }
    public required float BounceDamageMultiplier { get; init; }
}