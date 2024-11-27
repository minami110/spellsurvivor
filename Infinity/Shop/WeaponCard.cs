﻿using System;
using fms.Faction;
using Godot;

namespace fms;

/// <summary>
/// Shop で販売している Weapon Card
/// プレイヤーなど Entity が所有している場合は、その Entity の子ノードとして配置され,
/// 未所有の場合は Shop で管理しているメモリ上にのみ存在する
/// </summary>
public partial class WeaponCard : Node
{
    [Export]
    private MinionCoreData CoreData { get; set; } = null!;

    private IEntity? _ownedEntity;

    public string FriendlyName => CoreData.Name;

    public int Tier => CoreData.Tier;

    public uint Price => CoreData.Price;

    public Texture2D Sprite => CoreData.Sprite;

    public string Description => CoreData.Description;

    public WeaponBase Weapon { get; private set; } = null!;

    /// <summary>
    /// この Minion の所属する Faction (Flag)
    /// </summary>
    public FactionType Faction => Weapon.Faction;

    // parameterless constructor is required for Godot
    private WeaponCard()
    {
    }

    public WeaponCard(MinionCoreData data)
    {
        CoreData = data;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Set Group
            AddToGroup(Constant.GroupNameMinion);

            // Spawn Weapon
            Weapon = CoreData.WeaponPackedScene.Instantiate<WeaponBase>();
            Weapon.AutoStart = false;
            AddSibling(Weapon);
        }
        else if (what == NotificationExitTree)
        {
            // Remove Weapon
            Weapon.QueueFree();
        }
    }

    internal void OnBuy(IEntity entity)
    {
        // 違うエンティティが所有している場合はエラー
        if (_ownedEntity != null && _ownedEntity != entity)
        {
            throw new InvalidProgramException($"{nameof(WeaponCard)} is owned by another entity");
        }

        // プレイヤーのお金を減らす
        var state = entity.State;
        state.ReduceMoney(Price);

        // すでに同じエンティティが所有している場合はレベルアップ
        if (_ownedEntity == entity)
        {
            AddWeaponLevel(1u);
            return;
        }

        // 新規購入
        _ownedEntity = entity;
        _ownedEntity.AddChild(this);
    }

    internal void OnSell(IEntity entity)
    {
        if (_ownedEntity == null)
        {
            throw new InvalidProgramException($"{nameof(WeaponCard)} is not owned by anyone");
        }

        if (_ownedEntity != entity)
        {
            throw new InvalidProgramException($"{nameof(WeaponCard)} is owned by another entity");
        }

        _ownedEntity.RemoveChild(this);
        _ownedEntity = null;

        // プレイヤーのお金を増やす
        var state = entity.State;
        state.AddMoney(Price); // ToDo: 売値を設定する
    }

    private void AddWeaponLevel(uint level)
    {
        Weapon.State.SetLevel(Weapon.State.Level.CurrentValue + level);
    }
}