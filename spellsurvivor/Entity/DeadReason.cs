namespace fms;

public readonly struct DeadReason
{
    public readonly string Instigator;
    public readonly string By;

    public DeadReason(string instigator, string by)
    {
        Instigator = instigator;
        By = by;
    }
}