namespace fms.Effect;

/// <summary>
/// Entity Effect
/// https://scrapbox.io/FUMOSurvivor/Dodge
/// </summary>
public class Dodge : EffectBase
{
    /// <summary>
    /// Dodge の発生率 (0 ~ 1)
    /// </summary>
    public required float Rate { get; init; }
}