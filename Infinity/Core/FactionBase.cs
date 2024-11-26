using System;
using System.Collections.Generic;
using Godot;

namespace fms.Faction;

/// <summary>
/// Entity が所有する シナジー の基底クラス
/// </summary>
public partial class FactionBase : Node
{
    /// <summary>
    /// 現在の Faction の Level
    /// </summary>
    [Export]
    public uint Level
    {
        get;
        set
        {
            var newLevel = Math.Clamp(value, 0, Constant.FACTION_MAX_LEVEL);
            if (field == newLevel)
            {
                return;
            }

            field = newLevel;

            // Ready 前 (Editor での代入) の場合は終わり
            if (!IsNodeReady())
            {
                return;
            }

            // レベルアップ時に以前のレベルまでに発行済のエフェクトすべてを削除する
            foreach (var effect in _publishedEffects)
            {
                effect.QueueFree();
            }

            _publishedEffects.Clear();
            OnLevelChanged(newLevel);
        }
    }

    private readonly List<EffectBase> _publishedEffects = new();

    /// <summary>
    /// この Faction でなんらかの効果が有効になっているかどうか
    /// UI がこの値に応じて表示を切り替える
    /// </summary>
    public virtual bool IsActiveAnyEffect => Level >= 2u;

    public override void _Notification(int what)
    {
        // OnEnterTree
        if (what == NotificationEnterTree)
        {
            // Name にクラス名を設定
            Name = GetType().Name;

            // Faction グループに追加
            if (!IsInGroup(Constant.GroupNameFaction))
            {
                AddToGroup(Constant.GroupNameFaction);
            }
        }
        else if (what == NotificationReady)
        {
            OnLevelChanged(Level);
        }
    }

    /// <summary>
    /// Player に対して Effect を追加
    /// </summary>
    /// <param name="effect"></param>
    private protected void AddEffactToPlayer(EffectBase effect)
    {
        // 発行済みエフェクトとしてマークしておく
        _publishedEffects.Add(effect);

        // PlayerState の子にに Effect を追加
        var ps = GetParent().FindFirstChild<EntityState>();
        if (ps is null)
        {
            throw new ApplicationException("Failed to find PlayerState");
        }

        ps.AddChild(effect);
    }

    private protected void AddEffectToWeapon(WeaponBase weapon, EffectBase effect)
    {
        // 発行済みエフェクトとしてマークしておく
        _publishedEffects.Add(effect);

        // Weapon に Effect を追加
        weapon.AddChild(effect);
    }

    /// <summary>
    /// プレイヤーが装備を切り替えて最終的に Faction の Level が確定したときに呼ばれるコールバック
    /// 継承先で Effect を適用させる
    /// </summary>
    /// <param name="level"></param>
    private protected virtual void OnLevelChanged(uint level)
    {
    }
}