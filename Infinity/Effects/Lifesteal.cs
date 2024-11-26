namespace fms.Effect;

public partial class Lifesteal : EffectBase
{
    private const string _KEY_AMOUNT = "LifestealAmount";
    private const string _KEY_RATE = "LifestealRate";
    public required uint Amount { get; init; }
    public required float Rate { get; init; }

    public override void _EnterTree()
    {
        if (Dictionary.TryGetAttribute(_KEY_AMOUNT, out var v))
        {
            Dictionary.SetAttribute(_KEY_AMOUNT, (uint)v + Amount);
        }
        else
        {
            Dictionary.SetAttribute(_KEY_AMOUNT, Amount);
        }

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
        if (Dictionary.TryGetAttribute(_KEY_AMOUNT, out var v))
        {
            Dictionary.SetAttribute(_KEY_AMOUNT, (uint)v - Amount);
        }

        if (Dictionary.TryGetAttribute(_KEY_RATE, out var v2))
        {
            Dictionary.SetAttribute(_KEY_RATE, (float)v2 - Rate);
        }
    }
}