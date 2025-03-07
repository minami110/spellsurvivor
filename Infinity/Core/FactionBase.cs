﻿using System;
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
                if (IsInstanceValid(effect))
                {
                    effect.QueueFree();
                }
            }

            _publishedEffects.Clear();
            OnLevelChanged(newLevel);
        }
    }

    private readonly HashSet<EffectBase> _publishedEffects = new();

    private protected EntityState OwnerState
    {
        get
        {
            var entityState = GetParent().FindFirstChild<EntityState>();
            if (entityState is null)
            {
                throw new InvalidProgramException($"{nameof(EntityState)} is not found in siblings.");
            }

            return entityState;
        }
    }

    private protected IEnumerable<WeaponBase> SiblingWeapons
    {
        get
        {
            var siblings = GetParent().GetChildren();
            foreach (var sibling in siblings)
            {
                if (sibling is WeaponBase weapon)
                {
                    yield return weapon;
                }
            }
        }
    }

    public virtual string MainDescription => "Faction Description";

    public virtual IDictionary<uint, string> LevelDescriptions =>
        new Dictionary<uint, string>
        {
            { 2u, "Level 2 Description" },
            { 3u, "Level 3 Description" },
            { 5u, "Level 5 Description" }
        };

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
            // すでに兄弟に 同じ Faction が存在する場合はそっちに合体して自分は消える
            var siblings = GetParent().GetChildren();
            foreach (var sibling in siblings)
            {
                if (sibling is FactionBase faction && faction != this && faction.GetType() == GetType())
                {
                    faction.Level += Level;
                    QueueFree();
                    return;
                }
            }

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
        OwnerState.AddChild(effect);
    }

    private protected void AddEffectToWeapon(WeaponBase weapon, EffectBase effect)
    {
        // 発行済みエフェクトとしてマークしておく
        _publishedEffects.Add(effect);

        // Weapon に Effect を追加
        weapon.State.AddChild(effect);
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