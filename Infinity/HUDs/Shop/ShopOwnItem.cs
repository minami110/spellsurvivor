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

    public MinionInRuntime Minion { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = Minion.Sprite;
        _name.Text = Minion.Name;

        // Subscribe level
        var d1 = Minion.Level.Subscribe(this, (x, t) => { t._level.Text = $"(Lv.{x})"; });

        // ToDo: とりあえず買値と同じに..
        _sellButton.Text = $"Sell ${Minion.Price}";
        var d2 = _sellButton.PressedAsObservable().Subscribe(_ => { Main.ShopState.SellItem(Minion); });

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
        ToolTipToast.Text = Minion.Description;
        ToolTipToast.Show();
    }
}