using fms;
using Godot;
using R3;

/// <summary>
/// Shop で販売しているアイテム の HUD, Shop により自動で生成される
/// </summary>
public partial class ShopSellingItem : VBoxContainer
{
    [ExportGroup("Internal Reference")]
    [Export]
    private TextureRect _iconTextureRect = null!;

    [Export]
    private Label _nameLabel = null!;

    [Export]
    private Button _buyButton = null!;

    [Export]
    private Control _toolTipControl = null!;

    private bool _isSoldOut;

    public WeaponCard WeaponCard { get; internal set; } = null!;

    public override void _Ready()
    {
        _iconTextureRect.Texture = WeaponCard.Sprite;
        _nameLabel.Text = WeaponCard.FriendlyName;
        _buyButton.Text = $"${WeaponCard.Price}";

        // Subscribe 
        var d1 = _buyButton.PressedAsObservable().Subscribe(this, (_, t) => t.OnPressedBuyButton());
        var playerState = (EntityState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        var d2 = playerState.Money.ChangedCurrentValue.Subscribe(OnChangedPlayerMoney);

        Disposable.Combine(d1, d2).AddTo(this);

        // Tooltip
        _toolTipControl.MouseEntered += ShowToolTip;
        _toolTipControl.MouseExited += HideToolTip;
    }

    private void HideToolTip()
    {
        if (_isSoldOut)
        {
            return;
        }

        ToolTipToast.Hide();
    }

    private void OnChangedPlayerMoney(uint money)
    {
        if (money < WeaponCard.Price)
        {
            _buyButton.Modulate = new Color(1, 0, 0);
            _buyButton.Disabled = true;
            _buyButton.MouseDefaultCursorShape = CursorShape.Arrow;
        }
        else
        {
            _buyButton.Modulate = new Color(0, 1, 0);
            _buyButton.Disabled = false;
            _buyButton.MouseDefaultCursorShape = CursorShape.PointingHand;
        }
    }

    private void OnPressedBuyButton()
    {
        // GameMode に通知する
        Main.Shop.BuyWeaponCard(WeaponCard);
    }

    private void ShowToolTip()
    {
        if (_isSoldOut)
        {
            return;
        }

        var text = $"{WeaponCard.FriendlyName}\n";
        text += $"Tier: {WeaponCard.Tier}\n";
        text += $"Faction: ${WeaponCard.Faction}\n";
        text += $"{WeaponCard.Description}\n";

        ToolTipToast.Text = text;
        ToolTipToast.Show();
    }
}