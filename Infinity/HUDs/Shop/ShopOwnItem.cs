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

    public WeaponCard WeaponCard { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = WeaponCard.Sprite;
        _name.Text = WeaponCard.FriendlyName;

        // Subscribe level
        var d1 = WeaponCard.Weapon.State.Level.ChangedCurrentValue
            .Subscribe(this, (x, t) => { t._level.Text = $"(Lv.{x})"; });

        // ToDo: とりあえず買値と同じに..
        _sellButton.Text = $"Sell ${WeaponCard.Price}";
        var d2 = _sellButton.PressedAsObservable().Subscribe(_ => { Main.Shop.SellItem(WeaponCard); });

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
        var text = $"Tier: {WeaponCard.Tier}\n";
        text += $"Faction: ${WeaponCard.Faction}\n";
        text += $"{WeaponCard.Description}\n";

        ToolTipToast.Text = text;
        ToolTipToast.Show();
    }
}