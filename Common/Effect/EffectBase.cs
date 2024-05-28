namespace fms;

public abstract class EffectBase
{
    public required float Value { get; init; }
}

public class AddHealthEffect : EffectBase
{
}

public class AddMaxHealthEffect : EffectBase
{
}

public class AddMoneyEffect : EffectBase
{
}

public class PhysicalDamageEffect : EffectBase
{
}