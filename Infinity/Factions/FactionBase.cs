using System.Collections.Generic;
using Godot;

namespace fms.Faction;

/// <summary>
///     Entity が所有する シナジー の基底クラス
/// </summary>
public partial class FactionBase : Node
{
    /// <summary>
    ///     現在の Faction の Level
    /// </summary>
    [Export]
    public uint Level { get; private set; }

    private readonly List<EffectBase> _publishedEffects = new();

    /// <summary>
    ///     この Faction でなんらかの効果が有効になっているかどうか
    ///     UI がこの値に応じて表示を切り替える
    /// </summary>
    public virtual bool IsActiveAnyEffect => Level >= 2u;

    public override void _Notification(int what)
    {
        // OnEnterTree
        if (what == NotificationEnterTree)
        {
            // Faction グループに追加
            if (!IsInGroup(Constant.GroupNameFaction))
            {
                AddToGroup(Constant.GroupNameFaction);
            }
        }
    }

    public void ResetLevel()
    {
        SetLevel(0);
    }

    public void UpgradeLevel(uint amount = 1)
    {
        SetLevel(Level + amount);
    }

    /// <summary>
    /// Player に対して Effect を追加
    /// </summary>
    /// <param name="effect"></param>
    private protected void AddEffactToPlayer(EffectBase effect)
    {
        // 発行済みエフェクトとしてマークしておく
        _publishedEffects.Add(effect);

        // PlayerState に Effect を追加
        Main.PlayerState.AddEffect(effect);
    }

    /// <summary>
    ///     プレイヤーが装備を切り替えて最終的に Faction の Level が確定したときに呼ばれるコールバック
    ///     継承先で Effect を適用させる
    /// </summary>
    /// <param name="level"></param>
    private protected virtual void OnLevelChanged(uint level)
    {
    }

    private void SetLevel(uint level)
    {
        if (Level == level)
        {
            return;
        }

        // 発行済のエフェクトすべてを削除する
        foreach (var effect in _publishedEffects)
        {
            effect.Dispose();
        }

        _publishedEffects.Clear();

        Level = level;
        OnLevelChanged(Level);
    }
}