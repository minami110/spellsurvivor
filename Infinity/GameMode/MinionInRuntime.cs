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
public sealed class MinionInRuntime : IDisposable
{
    private const uint _MIN_LEVEL = 1;
    private const uint _MAX_LEVEL = 5;

    private readonly ReactiveProperty<uint> _levelRp = new(1);

    /// <summary>
    /// </summary>
    public readonly string Id;

    private WeaponBase? _weapon;

    private IDisposable? _weaponSubscription;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<uint> Level => _levelRp;

    public bool IsMaxLevel => _levelRp.Value >= _MAX_LEVEL;

    public int Tier { get; }

    public int Price { get; }

    public Texture2D Sprite { get; }

    public string Name { get; }

    public string Description { get; }

    public PackedScene WeaponPackedScene { get; }

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
    public FactionType Faction { get; }

    /// <summary>
    ///     現在の Minion の場所
    /// </summary>
    public MinionPlace Place { get; set; }

    public MinionInRuntime(MinionCoreData minionCoreData)
    {
        Id = minionCoreData.Id;
        Price = minionCoreData.Price;
        Faction = minionCoreData.Faction;
        Tier = minionCoreData.Tier;
        Name = minionCoreData.Name;
        Description = minionCoreData.Description;
        Sprite = minionCoreData.Sprite;
        WeaponPackedScene = minionCoreData.WeaponPackedScene;
    }

    public void ResetRuntimeStatus()
    {
        Place = MinionPlace.InShop;
        Weapon = null;
        SetLevel(1);
    }


    public void SetLevel(uint level)
    {
        _levelRp.Value = Math.Clamp(level, _MIN_LEVEL, _MAX_LEVEL);
    }


    public void Dispose()
    {
        _levelRp.Dispose();
        _weaponSubscription?.Dispose();
        _weaponSubscription = null;
    }
}