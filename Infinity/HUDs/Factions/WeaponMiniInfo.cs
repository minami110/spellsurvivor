using fms.Faction;
using Godot;

namespace fms.HUD;

public partial class WeaponMiniInfo : Control
{
    public WeaponCard? Weapon
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Weapon is null)
        {
            Hide();
            return;
        }

        // Name
        GetNode<Label>("%Name").Text = Weapon.FriendlyName;

        // Factions
        var slot = 0;
        var maxSlot = 3;
        var weaponFactions = Weapon.Faction;
        foreach (var f in FactionUtil.GetFactionTypes())
        {
            if (slot >= maxSlot)
            {
                break;
            }

            if (weaponFactions.HasFlag(f))
            {
                var l = GetNode<Label>($"%Faction{slot}");
                l.Text = $"FACTION_{f.ToString().ToUpper()}";
                l.Show();
                slot++;
            }
        }

        for (var i = slot; i < maxSlot; i++)
        {
            GetNode<Label>($"%Faction{i}").Hide();
        }

        ResetSize();
        Show();
    }
}