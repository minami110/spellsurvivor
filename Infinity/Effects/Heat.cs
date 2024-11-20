namespace fms.Effect;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Heat
/// </summary>
public class Heat : EffectBase
{
    public required uint Range { get; init; }
    public required uint Span { get; init; }
    public required uint Damage { get; init; }
}