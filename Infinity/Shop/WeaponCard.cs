using fms.Faction;
using Godot;

namespace fms;

/// <summary>
/// Shop で販売している Weapon
/// </summary>
public partial class WeaponCard : Node
{
    [Export]
    private MinionCoreData CoreData { get; set; } = null!;

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

    public void AddWeaponLevel(uint level)
    {
        Weapon.State.SetLevel(Weapon.State.Level.CurrentValue + level);
    }
}