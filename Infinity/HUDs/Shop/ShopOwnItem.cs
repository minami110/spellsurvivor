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

    [Export]
    private Button _sellButton = null!;

    [Export]
    private Control _toolTipControl = null!;

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

        // ToDo: とりあえず買値と同じに..
        _sellButton.Text = $"Sell ${ItemSettings.Price}";
        var d2 = _sellButton.PressedAsObservable().Subscribe(_ => { Main.GameMode.SellItem(ItemSettings); });

        // Tooltip
        _toolTipControl.MouseEntered += ShowToolTip;
        _toolTipControl.MouseExited += HideToolTip;

        Disposable.Combine(d1, d2).AddTo(this);
    }

    private void HideToolTip()
    {
        ToolTipToast.Hide();
    }

    private void ShowToolTip()
    {
        ToolTipToast.Text = ItemSettings.Description;
        ToolTipToast.Show();
    }
}