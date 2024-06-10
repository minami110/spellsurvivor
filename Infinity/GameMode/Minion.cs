using System;
using fms.Faction;
using fms.Weapon;
using Godot;
using R3;

namespace fms;

/// <summary>
///     Player が所有済みの Minion の情報
/// </summary>
[GlobalClass]
public partial class Minion : Node
{
    [Export]
    private MinionCoreData CoreData { get; set; } = null!;

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

    public string FriendlyName => CoreData.Name;

    public int Tier => CoreData.Tier;

    public int Price => CoreData.Price;

    public Texture2D Sprite => CoreData.Sprite;

    public string Description => CoreData.Description;

    /// <summary>
    ///     この Minion が装備している Weapon, InHand にない場合は所有していない
    /// </summary>
    public WeaponBase? Weapon
    {
        get => _weapon;
        private set
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

    private Minion()
    {
    }

    public Minion(MinionCoreData data)
    {
        CoreData = data;
    }

    public override void _EnterTree()
    {
        // Set Name (for debugging)
        Name = $"(Minion) {CoreData.Id}";

        // Set Group
        if (!IsInGroup(Constant.GroupNameMinion))
        {
            AddToGroup(Constant.GroupNameMinion);
        }
    }

    public override void _ExitTree()
    {
        RemoveWeapon();
    }

    public void AddWeapon()
    {
        // Spawn Weapon
        if (Weapon is null)
        {
            Weapon = CoreData.WeaponPackedScene.Instantiate<WeaponBase>();
            Weapon!.Id = Id;
            Weapon!.Level = _levelRp.Value;
            AddSibling(Weapon);
        }
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

    public void RemoveWeapon()
    {
        if (Weapon is null)
        {
            return;
        }

        Weapon.QueueFree();
        Weapon = null;
    }

    public void ResetRuntimeStatus()
    {
        RemoveWeapon();
        SetLevel(1);
    }

    public void SetLevel(uint level)
    {
        _levelRp.Value = Math.Clamp(level, Constant.MINION_MIN_LEVEL, Constant.MINION_MAX_LEVEL);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _levelRp.Dispose();
    }
}