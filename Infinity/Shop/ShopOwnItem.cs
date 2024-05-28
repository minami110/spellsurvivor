using Godot;

namespace fms;

public partial class ShopOwnItem : VBoxContainer
{
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    public ShopItemSettings ItemSettings { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = ItemSettings.Icon;
        _name.Text = ItemSettings.Name;
    }
}