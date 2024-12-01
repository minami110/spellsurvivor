using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class FactionInfo : Control
{
    [Export]
    private uint _weaponMiniThumbnailCount = 8u;

    public FactionType Faction
    {
        get;
        set
        {
            field = value;
            OnChangedFaction();
        }
    }

    public override void _Ready()
    {
        for (var i = 0; i < _weaponMiniThumbnailCount; i++)
        {
            var thumbnail = GetNode<WeaponMiniThumbnail>($"%WeaponMiniThumbnail{i}");
            thumbnail.RequestShowInfo
                .Subscribe(ShowWeaponInfo)
                .AddTo(this);
            thumbnail.RequestHideInfo
                .Subscribe(HideWeaponInfo)
                .AddTo(this);
        }

        HideWeaponInfo(Unit.Default);
    }

    private void HideWeaponInfo(Unit _)
    {
        GetNode<WeaponMiniInfo>("%WeaponMiniInfo").Weapon = null;
    }


    private void OnChangedFaction()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Faction == 0u)
        {
            Hide();
            return;
        }

        // Faction Name
        GetNode<Label>("%Name").Text = $"FACTION_{Faction.ToString().ToUpper()}";

        // Faction Sprite
        GetNode<TextureRect>("%Sprite").Texture = Faction.GetTextureResouce();

        // ToDo: Faction Description

        // ToDo: Faction Level Description

        // Weapons list that belongs to this faction
        var weapons = Main.Shop.GetWeaponsBelongTo(Faction);
        for (var i = 0; i < _weaponMiniThumbnailCount; i++)
        {
            var thumbnail = GetNode<WeaponMiniThumbnail>($"%WeaponMiniThumbnail{i}");
            if (i < weapons.Count)
            {
                thumbnail.WeaponCard = weapons[i];
            }
            else
            {
                thumbnail.WeaponCard = null;
            }
        }


        Show();
    }

    private void ShowWeaponInfo(WeaponCard weapon)
    {
        GetNode<WeaponMiniInfo>("%WeaponMiniInfo").Weapon = weapon;
    }
}