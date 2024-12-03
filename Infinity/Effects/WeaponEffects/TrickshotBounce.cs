namespace fms.Effect;

/// <summary>
/// Faction: Trickshot により生成される Effect
/// </summary>
public partial class TrickshotBounce : EffectBase
{
    private const string _KEY_COUNT = WeaponAttributeNames.BounceCount;
    private const string _KEY_MULTIPLIER = WeaponAttributeNames.BounceDamageRate;
    public required uint BounceCount { get; init; }
    public required float BounceDamageRate { get; init; }

    public override void _EnterTree()
    {
        if (Dictionary.TryGetAttribute(_KEY_COUNT, out var v))
        {
            Dictionary.SetAttribute(_KEY_COUNT, (uint)v + BounceCount);
        }
        else
        {
            Dictionary.SetAttribute(_KEY_COUNT, BounceCount);
        }

        if (Dictionary.TryGetAttribute(_KEY_MULTIPLIER, out var v2))
        {
            Dictionary.SetAttribute(_KEY_MULTIPLIER, (float)v2 + BounceDamageRate);
        }
        else
        {
            Dictionary.SetAttribute(_KEY_MULTIPLIER, BounceDamageRate);
        }
    }

    public override void _ExitTree()
    {
        if (Dictionary.TryGetAttribute(_KEY_COUNT, out var v))
        {
            Dictionary.SetAttribute(_KEY_COUNT, (uint)v - BounceCount);
        }

        if (Dictionary.TryGetAttribute(_KEY_MULTIPLIER, out var v2))
        {
            Dictionary.SetAttribute(_KEY_MULTIPLIER, (float)v2 - BounceDamageRate);
        }
    }
}