using fms.Minion;
using Godot;
using R3;

namespace fms;

public partial class ShopOwnItem : VBoxContainer
{
    [ExportGroup("Internal Reference")]
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    [Export]
    private Label _level = null!;

    private MinionBase _minion = null!;

    public MinionCoreData ItemSettings { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = ItemSettings.Icon;
        _name.Text = ItemSettings.Name;

        // Subscribe level
        _minion = Main.GameMode.Minions[ItemSettings];
        var d1 = _minion.Level.Subscribe(this, (x, t) =>
        {
            if (t._minion.MaxLevel == x)
            {
                t._level.Text = "(Max)";
            }
            else
            {
                t._level.Text = $"(Lv.{x})";
            }
        });
    }
}