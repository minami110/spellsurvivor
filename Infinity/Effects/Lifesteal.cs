namespace fms.Effect;

public class Lifesteal : EffectBase
{
    public required uint Amount { get; init; }
    public required float Rate { get; init; }
}