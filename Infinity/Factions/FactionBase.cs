namespace fms.Faction;

public abstract class FactionBase : IEffectSolver
{
    public virtual bool IsAnyEffectActive => Level >= 2;
    public int Level { get; private set; }

    // レベルを確定
    public void ConfirmLevel()
    {
        if (Level == 0)
        {
            return;
        }

        OnLevelConfirmed(Level);
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

    public void UpgradeLevel(int amount = 1)
    {
        Level += amount;
    }

    /// <summary>
    ///     プレイヤーが装備を切り替えて最終的に Faction の Level が確定したときに呼ばれるコールバック
    ///     継承先で Effect を適用させる
    /// </summary>
    /// <param name="level"></param>
    private protected virtual void OnLevelConfirmed(int level)
    {
    }

    /// <summary>
    ///     プレイヤーが装備を切り替えるさい, 最初に Level が 0 にリセットされるときに呼ばれるコールバック
    /// </summary>
    /// <param name="oldLevel"></param>
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