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

    public string FriendlyName => CoreData.Name;

    public int Tier => CoreData.Tier;

    public uint Price => CoreData.Price;

    public Texture2D Sprite => CoreData.Sprite;

    public string Description => CoreData.Description;

    /// <summary>
    /// この Minion が装備している Weapon, InHand にない場合は所有していない
    /// </summary>
    public WeaponBase Weapon { get; private set; } = null!;

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

            // Spawn Weapon
            SpawnWeapon();
        }
        else if (what == NotificationReady)
        {
            foreach (var faction in FactionUtil.GetFactionTypes())
            {
                if (!Weapon.IsBelongTo(faction))
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
                if (!Weapon.IsBelongTo(faction))
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
        }
    }

    public void AddWeaponLevel(uint level)
    {
        Weapon.State.SetLevel(Weapon.State.Level.CurrentValue + level);
    }

    private void SpawnWeapon()
    {
        Weapon = CoreData.WeaponPackedScene.Instantiate<WeaponBase>();
        Weapon.Faction = Faction;
        Weapon.AutoStart = false;
        AddSibling(Weapon);
    }
}