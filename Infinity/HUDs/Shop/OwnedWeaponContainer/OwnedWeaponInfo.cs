using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class OwnedWeaponInfo : Control
{
    public WeaponBase? Weapon
    {
        get;
        set
        {
            field = value;
            OnChangedWeapon();
        }
    }

    public override void _Ready()
    {
        OnChangedWeapon();

        // Faction Toast
        var count = 3;
        for (var i = 0; i < count; i++)
        {
            var factionLabel = GetNode<FacionLabel>($"%FactionLabel{i}");
            factionLabel.RequestShowInfo
                .Subscribe(ShowFactionInfo)
                .AddTo(this);

            factionLabel.RequestHideInfo
                .Subscribe(HideFactionInfo)
                .AddTo(this);
        }

        // Weapon Stat Toast
        var damage = GetNode<WeaponStatLabel>("%Damage");
        damage.RequestShowInfo
            .Subscribe(ShowStatInfo)
            .AddTo(this);
        damage.RequestHideInfo
            .Subscribe(HideStatInfo)
            .AddTo(this);

        HideFactionInfo(Unit.Default);
        HideStatInfo(Unit.Default);
    }

    private void HideFactionInfo(Unit _)
    {
        GetNode<FactionInfo>("%FactionInfo").Faction = 0u;
    }

    private void HideStatInfo(Unit _)
    {
        GetNode<Control>("%WeaponStatInfo").Hide();
    }

    private void OnChangedWeapon()
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

        var sprite = GetNode<TextureRect>("%WeaponSprite");
        sprite.Texture = Weapon.Config.Sprite;
        var name = GetNode<Label>("%WeaponName");
        name.Text = Weapon.Config.Name;
        var level = GetNode<Label>("%WeaponLevel");
        level.Text = $"Lv. {Weapon.State.Level.CurrentValue}";
        var desc = GetNode<RichTextLabel>("%WeaponDescription");
        desc.Text = Weapon.Config.Description;

        // Factions
        var weaponFaction = Weapon.Config.Faction;
        var max = 3;
        var i = 0;
        foreach (var f in FactionUtil.GetFactionTypes())
        {
            if (i >= max)
            {
                break;
            }

            if (weaponFaction.HasFlag(f))
            {
                var label = GetNode<FacionLabel>($"%FactionLabel{i}");
                label.Faction = f;
                i++;
            }
        }

        for (var j = i; j < max; j++)
        {
            var label = GetNode<FacionLabel>($"%FactionLabel{j}");
            label.Faction = 0u;
        }

        // Stats
        var damage = GetNode<WeaponStatLabel>("%Damage");
        damage.DefaultValue = Weapon.State.Damage.DefaultValue;
        damage.CurrentValue = Weapon.State.Damage.CurrentValue;

        Show();
    }

    private void ShowFactionInfo(FactionType faction)
    {
        GetNode<FactionInfo>("%FactionInfo").Faction = faction;
    }

    private void ShowStatInfo(string statType)
    {
        GetNode<Control>("%WeaponStatInfo").Show();
    }
}