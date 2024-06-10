using System.Collections.Generic;

namespace fms.Faction;

public abstract class FactionBase
{
    private readonly List<EffectBase> _publishedEffects = new();

    /// <summary>
    ///     この Faction でなんらかの効果が有効になっているかどうか
    ///     UI がこの値に応じて表示を切り替える
    /// </summary>
    public virtual bool IsActiveAnyEffect => Level >= 2;

    /// <summary>
    ///     現在の Faction の Level
    /// </summary>
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

        Level = 0;

        if (_publishedEffects.Count == 0)
        {
            return;
        }

        // 発行済のエフェクトすべてを削除する
        foreach (var effect in _publishedEffects)
        {
            effect.Dispose();
        }

        _publishedEffects.Clear();
    }

    public void UpgradeLevel(int amount = 1)
    {
        Level += amount;
    }

    private protected void OnEffectPublished(EffectBase effect)
    {
        _publishedEffects.Add(effect);
    }

    /// <summary>
    ///     プレイヤーが装備を切り替えて最終的に Faction の Level が確定したときに呼ばれるコールバック
    ///     継承先で Effect を適用させる
    /// </summary>
    /// <param name="level"></param>
    private protected virtual void OnLevelConfirmed(int level)
    {
    }
}