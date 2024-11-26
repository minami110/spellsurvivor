namespace fms.Effect;

/// <summary>
/// Weapon Effect: Strength
/// DamageRate を増やす
/// https://scrapbox.io/FUMOSurvivor/Strength
/// </summary>
public partial class Strength : EffectBase
{
    public required float Rate { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = WeaponAttributeNames.DamageRate;

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
        const string _KEY = WeaponAttributeNames.DamageRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v - Rate);
        }
    }
}