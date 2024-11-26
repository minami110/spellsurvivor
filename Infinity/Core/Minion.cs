using fms.Faction;
using Godot;

namespace fms;

/// <summary>
/// Shop アイテム
/// </summary>
[GlobalClass]
public partial class Minion : Node
{
    [Export]
    private MinionCoreData CoreData { get; set; } = null!;

    private WeaponBase? _weapon;

    /// <summary>
    /// </summary>
    public string Id => CoreData.Id;

    public bool IsMaxLevel => _levelRp.Value >= Constant.MINION_MAX_LEVEL;

    public string FriendlyName => CoreData.Name;

    public int Tier => CoreData.Tier;

    public uint Price => CoreData.Price;

    public Texture2D Sprite => CoreData.Sprite;

    public string Description => CoreData.Description;

    /// <summary>
    /// この Minion が装備している Weapon, InHand にない場合は所有していない
    /// </summary>
    public WeaponBase? Weapon
    {
        get => _weapon;
        private set
        {
            _weapon = value;

            // 新しい装備
            if (_weapon is not null)
            {
                _weapon.MinionId = Id;
            }
        }
    }

    /// <summary>
    /// この Minion の所属する Faction (Flag)
    /// </summary>
    public FactionType Faction => CoreData.Faction;

    private Minion()
    {
    }

    public Minion(MinionCoreData data)
    {
        CoreData = data;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Set Name (for debugging)
            Name = $"(Minion) {CoreData.Id}";

            // Set Group
            if (!IsInGroup(Constant.GroupNameMinion))
            {
                AddToGroup(Constant.GroupNameMinion);
            }

            // Set Level
            SetWeaponLevel(1);

            // Spawn Weapon
            SpawnWeapon();
        }
        else if (what == NotificationReady)
        {
            foreach (var faction in FactionUtil.GetFactionTypes())
            {
                if (!IsBelongTo(faction))
                {
                    continue;
                }

                // すでに Faction が存在していたらレベルを上げる
                var s = this.FindSibling("*", faction.ToString());
                if (s.Count > 0)
                {
                    var f = (FactionBase)s[0];
                    f.Level++;
                }
                else
                {
                    // Faction が存在していなかったら作成
                    var f = FactionUtil.CreateFaction(faction);
                    f.Level++;
                    AddSibling(f);
                }
            }
        }
        else if (what == NotificationExitTree)
        {
            foreach (var faction in FactionUtil.GetFactionTypes())
            {
                if (!IsBelongTo(faction))
                {
                    continue;
                }

                // すでに Faction が存在していたらレベルを下げる
                var s = this.FindSibling("*", faction.ToString());
                if (s.Count > 0)
                {
                    var f = (FactionBase)s[0];
                    f.Level--;
                }
            }

            RemoveWeapon();
            SetWeaponLevel(0);
        }
    }

    /// <summary>
    /// Minion が指定した Faction に所属しているかどうか
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

    public void SetWeaponLevel(uint level)
    {
        if (_weapon is not null)
        {
            _weapon.State.SetLevel(level);
        }
    }

    private void SpawnWeapon()
    {
        // まだ装備していない場合ときは生成する
        if (Weapon is not null)
        {
            return;
        }

        Weapon = CoreData.WeaponPackedScene.Instantiate<WeaponBase>();
        Weapon.MinionId = Id;
        Weapon.Faction = Faction;
        Weapon.AutoStart = false;
        AddSibling(Weapon);
    }
}