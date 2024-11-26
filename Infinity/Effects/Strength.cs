namespace fms.Effect;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Strength
/// </summary>
public partial class Strength : EffectBase
{
    public required uint Amount { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = WeaponAttributeNames.DamageRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v + Amount);
        }
        else
        {
            Dictionary.SetAttribute(_KEY, Amount);
        }
    }

    public override void _ExitTree()
    {
        const string _KEY = WeaponAttributeNames.DamageRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v - Amount);
        }
    }
}