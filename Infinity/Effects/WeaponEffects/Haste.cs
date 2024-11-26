namespace fms.Effect;

public partial class Haste : EffectBase
{
    public required float Amount { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = WeaponAttributeNames.SpeedRate;

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
        const string _KEY = WeaponAttributeNames.SpeedRate;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v - Amount);
        }
    }
}