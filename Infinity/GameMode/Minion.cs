using System;
using fms.Faction;
using fms.Weapon;
using Godot;
using R3;

namespace fms;

public enum MinionPlace
{
    InShop,
    InHand,
    InStorage
}

/// <summary>
///     Player が所有済みの Minion の情報
/// </summary>
[GlobalClass]
public partial class Minion : Node
{
    [Export]
    internal MinionCoreData CoreData { get; set; } = null!;

    private readonly ReactiveProperty<uint> _levelRp = new(1);

    private WeaponBase? _weapon;

    private IDisposable? _weaponSubscription;

    /// <summary>
    /// </summary>
    public string Id => CoreData.Id;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<uint> Level => _levelRp;

    public bool IsMaxLevel => _levelRp.Value >= Constant.MINION_MAX_LEVEL;

    public int Tier => CoreData.Tier;

    public int Price => CoreData.Price;

    public Texture2D Sprite => CoreData.Sprite;

    public new string Name => CoreData.Name;

    public string Description => CoreData.Description;

    public PackedScene WeaponPackedScene => CoreData.WeaponPackedScene;

    /// <summary>
    ///     この Minion が装備している Weapon, InHand にない場合は所有していない
    /// </summary>
    public WeaponBase? Weapon
    {
        get => _weapon;
        set
        {
            // 前の装備
            if (_weapon is not null)
            {
                _weaponSubscription?.Dispose();
                _weaponSubscription = null;
            }

            _weapon = value;

            // 新しい装備
            if (_weapon is not null)
            {
                _weapon.Id = Id;
                _weaponSubscription = _levelRp.Subscribe(x => { _weapon.Level = x; });
            }
        }
    }

    /// <summary>
    ///     この Minion の所属する Faction (Flag)
    /// </summary>
    public FactionType Faction => CoreData.Faction;

    /// <summary>
    ///     現在の Minion の場所
    /// </summary>
    public MinionPlace Place { get; set; }


    public override void _ExitTree()
    {
        _levelRp.Dispose();
        _weaponSubscription?.Dispose();
        _weaponSubscription = null;
    }

    /// <summary>
    ///     Minion が指定した Faction に所属しているかどうか
    /// </summary>
    /// <param name="faction"></param>
    /// <returns></returns>
    public bool IsBelongTo(FactionType faction)
    {
        return Faction.HasFlag(faction);
    }

    public void ResetRuntimeStatus()
    {
        Place = MinionPlace.InShop;
        Weapon = null;
        SetLevel(1);
    }

    public void SetLevel(uint level)
    {
        _levelRp.Value = Math.Clamp(level, Constant.MINION_MIN_LEVEL, Constant.MINION_MAX_LEVEL);
    }
}