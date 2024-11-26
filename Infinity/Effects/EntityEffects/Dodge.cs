namespace fms.Effect;

/// <summary>
/// Entity Effect: Dodge
/// Entity の回避率 (Dodge Rate
/// ) を増やす
/// https://scrapbox.io/FUMOSurvivor/Dodge
/// </summary>
public partial class Dodge : EffectBase
{
    /// <summary>
    /// Dodge の発生率 (0 ~ 1)
    /// </summary>
    public required float Rate { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = EntityAttributeNames.DodgeRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v + Rate);
        }
        else
        {
            Dictionary.SetAttribute(_KEY, Rate);
        }
    }

    public override void _ExitTree()
    {
        const string _KEY = EntityAttributeNames.DodgeRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v - Rate);
        }
    }
}