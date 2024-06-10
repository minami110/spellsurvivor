namespace fms;

/// <summary>
///     物理ダメージを発生させる Effect
/// </summary>
public class PhysicalDamageEffect : EffectBase
{
    public required float Value { get; init; }
}