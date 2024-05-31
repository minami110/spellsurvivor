namespace fms.Faction;

public abstract class FactionBase : IEffectSolver
{
    public virtual bool IsAnyEffectActive => Level >= 2;
    public int Level { get; private set; }

    public void AddLevel(int amount = 1)
    {
        Level += amount;
    }

    // レベルを確定
    public void ConfirmLevel()
    {
        if (Level == 0)
        {
            return;
        }

        OnLevelConfirmed(Level);
    }

    public virtual void OnBattleStarted()
    {
    }

    public void ResetLevel()
    {
        if (Level == 0)
        {
            return;
        }

        var oldLevel = Level;
        Level = 0;
        OnLevelReset(oldLevel);
    }

    private protected virtual void OnLevelConfirmed(int level)
    {
    }

    private protected virtual void OnLevelReset(int oldLevel)
    {
    }

    public void AddEffect(EffectBase effect)
    {
    }

    public void SolveEffect()
    {
    }
}

/// <summary>
///     Lv2: 10秒毎 に一時的なシールド "10" を得る
///     Lv4: 10秒毎 に一時的なシールド "25" を得る
///     Lv6: 10秒毎 に一時的なシールド "25 + Player" の最大体力の 2% を得る
/// </summary>
public sealed class Knight : FactionBase
{
}