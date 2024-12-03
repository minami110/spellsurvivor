namespace fms.Effect;

public partial class Lifesteal : EffectBase
{
    private const string _KEY_RATE = WeaponAttributeNames.LifestealRate;
    public required float Rate { get; init; }

    public override void _EnterTree()
    {
        if (Dictionary.TryGetAttribute(_KEY_RATE, out var v2))
        {
            Dictionary.SetAttribute(_KEY_RATE, (float)v2 + Rate);
        }
        else
        {
            Dictionary.SetAttribute(_KEY_RATE, Rate);
        }
    }

    public override void _ExitTree()
    {
        if (Dictionary.TryGetAttribute(_KEY_RATE, out var v2))
        {
            Dictionary.SetAttribute(_KEY_RATE, (float)v2 - Rate);
        }
    }
}