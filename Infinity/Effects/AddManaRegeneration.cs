namespace fms.Effect;

/// <summary>
///     Faction: Trickshot により生成される Effect
/// </summary>
public class AddManaRegeneration : EffectBase
{
    public required int Value { get; init; }
    public required int Interval { get; init; }
}