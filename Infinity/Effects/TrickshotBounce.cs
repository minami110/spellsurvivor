namespace fms.Effect;

/// <summary>
///     Faction: Trickshot により生成される Effect
/// </summary>
public class TrickshotBounce : EffectBase
{
    public required int BounceCount { get; init; }
    public required float BounceDamageMultiplier { get; init; }
}