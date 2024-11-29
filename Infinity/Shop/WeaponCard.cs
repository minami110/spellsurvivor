using System;
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
    private readonly WeaponConfig _config;
    private readonly WeaponBase _weapon;

    private IEntity? _ownedEntity;

    public string FriendlyName => _config.Name;

    public TierType TierType => _config.Tier;

    public uint Price => _config.Price;

    public Texture2D Sprite => _config.Sprite;

    public string Description
    {
        get
        {
            var desc = _config.Description;
            desc += "\n\n";
            desc += $"Damage: {_config.Damage}\n";
            desc += $"Cooldown: {_config.Cooldown} frames\n";
            desc += $"Speed: {_config.CooldownRate}%\n";
            desc += $"Knockback: {_config.Knockback} px/s";
            return desc;
        }
    }

    /// <summary>
    /// この Minion の所属する Faction (Flag)
    /// </summary>
    public FactionType Faction => _config.Faction;


    // parameterless constructor is required for Godot
    private WeaponCard()
    {
        throw new ApplicationException($"Do not use {nameof(WeaponCard)} directly in Editor");
    }

    public WeaponCard(WeaponConfig config, WeaponBase weapon)
    {
        _config = config;
        _weapon = weapon;
    }

    public override void _Notification(int what)
    {
        // Note: プレイヤーに購入されたとき初めてツリーに入る
        if (what == NotificationEnterTree)
        {
            // Set Group
            AddToGroup(Constant.GroupNameMinion);

            // Weapon も一緒に兄弟に追加しておく
            _weapon.AutoStart = false;
            AddSibling(_weapon);
        }
        // Note: プレイヤーに売却されたときツリーから削除される
        else if (what == NotificationExitTree)
        {
            // Weapon も一緒にツリーから削除しておく
            GetParent().RemoveChild(_weapon);
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
            // ToDo: 最大レベルに達している場合はエラー
            var currentLevel = _weapon.State.Level.CurrentValue;
            _weapon.State.SetLevel(currentLevel + 1u);
            return;
        }

        // まだだれにも所有されていない場合は Entity の子ノードとして配置する
        // Note: ここでこのクラスの EnterTree が呼ばれる
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
}