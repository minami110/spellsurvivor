namespace fms.Effect;

/// <summary>
/// Entity Effect: Wing
/// Entity の移動速度を増やす
/// https://scrapbox.io/FUMOSurvivor/Wing
/// </summary>
public partial class Wing : EffectBase
{
    public required float Amount { get; init; }

    public override void _EnterTree()
    {
        const string _KEY = EntityAttributeNames.MoveSpeed;

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
        const string _KEY = EntityAttributeNames.MoveSpeed;

        if (Dictionary.TryGetAttribute(_KEY, out var v))
        {
            Dictionary.SetAttribute(_KEY, (float)v - Amount);
        }
    }
}