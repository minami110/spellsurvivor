using System.Text.RegularExpressions;
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
    public string FocusKey { get; private set; } = string.Empty;

    private int _weaponMiniThumbnailCount;

    public FactionType Faction
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    public override void _Ready()
    {
        var nodes = FindChildren("WeaponMiniThumbnail*");
        _weaponMiniThumbnailCount = nodes.Count;

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
        UpdateUi();
    }

    private void HideWeaponInfo(Unit _)
    {
        GetNode<WeaponMiniInfo>("%WeaponMiniInfo").Weapon = null;
    }

    private bool ReplaceStatToBbcode(string input, out string output)
    {
        // Syntax: <stat type=move_speed value=10 /> を置換する
        // Note: 4.3 時点で用意されている bbcode の拡張ではこういうことができない

        output = input;

        var pattern = @"<stat\s+(?<attributes>.*?)\/>";
        var match = Regex.Match(input, pattern);
        var type = string.Empty;
        var value = string.Empty;

        if (match.Success)
        {
            var attributesString = match.Groups["attributes"].Value;
            var attributes = attributesString.Split(' ');
            foreach (var attr in attributes)
            {
                var parts = attr.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                var k = parts[0];
                var v = parts[1];
                switch (k)
                {
                    case "type":
                        type = v;
                        break;
                    case "value":
                        value = v;
                        break;
                }
            }
        }

        if (type != string.Empty)
        {
            var tooltip = Tr($"STAT_{type.ToUpper()}");
            var path = $"res://base/textures/stats/{type}.png";
            output = input.Replace(match.Value, $"[img width=16 tooltip={tooltip}]{path}[/img] {value}");
            return true;
        }

        return false;
    }

    private void ShowWeaponInfo(WeaponCard weapon)
    {
        // 表示する前に WeaponMiniInfo の位置を調整する
        var firstThumb = GetNode<Control>("%WeaponMiniThumbnail0");
        var gridStartPositionY = firstThumb.GlobalPosition.Y;
        var weaponMiniInfo = GetNode<WeaponMiniInfo>("%WeaponMiniInfo");
        weaponMiniInfo.GlobalPosition = new Vector2(weaponMiniInfo.GlobalPosition.X, gridStartPositionY);

        // 表示する
        weaponMiniInfo.Weapon = weapon;
    }

    private void UpdateUi()
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
        var majorDesc = Tr(Faction.GetMajorDescription());
        while (ReplaceStatToBbcode(majorDesc, out var replaced))
        {
            majorDesc = replaced;
        }

        GetNode<RichTextLabel>("%Description").Text = majorDesc;

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
            var levelDesc = Tr(d);

            while (ReplaceStatToBbcode(levelDesc, out var replaced))
            {
                levelDesc = replaced;
            }

            label.Text = $"({l}) {levelDesc}";
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

        // 表示する
        GetNode<Control>("PanelContainer").ResetSize();
        Show();
    }
}