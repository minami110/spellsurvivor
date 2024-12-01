using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

/// <summary>
/// 所有している武器の Faction にフォーカスされた際に, Faction の詳細情報を表示する Toast HUD
/// </summary>
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

        // Faction major description
        GetNode<RichTextLabel>("%Description").Text = Faction.GetMajorDescription();

        // Faction level descriptions
        var factionLevel = 0u;
        {
            // ToDo: Player の Faction Level へのアクセスがややまどろっこしい
            var player = this.GetPlayerNode();
            foreach (var n in player.GetChildren())
            {
                if (n is not FactionBase faction)
                {
                    continue;
                }

                if (faction.GetFactionType() == Faction)
                {
                    factionLevel = faction.Level;
                }
            }
        }
        var maxSlot = 4;
        var currentSlot = 0;
        var levelDescriptions = Faction.GetLevelDescriptions();
        foreach (var (l, d) in levelDescriptions)
        {
            if (currentSlot >= maxSlot)
            {
                break;
            }

            var label = GetNode<RichTextLabel>($"%LevelDescription{currentSlot}");
            label.Text = $"({l}) {d}";

            label.Modulate = factionLevel < l ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f);

            label.Show();
            currentSlot++;
        }

        for (var i = currentSlot; i < maxSlot; i++)
        {
            GetNode<RichTextLabel>($"%LevelDescription{i}").Hide();
        }

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