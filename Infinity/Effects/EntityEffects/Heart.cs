namespace fms.Effect;

/// <summary>
/// Entity Effect: Heart
/// Entity の最大体力を増やす
/// https://scrapbox.io/FUMOSurvivor/Heart
/// </summary>
public partial class Heart : EffectBase
{
    public required uint Amount { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = EntityAttributeNames.MaxHealth;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (uint)v + Amount);
        }
        else
        {
            Dictionary.SetAttribute(_KEY, Amount);
        }
    }

    public override void _ExitTree()
    {
        const string _KEY = EntityAttributeNames.MaxHealth;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (uint)v - Amount);
        }
    }
}