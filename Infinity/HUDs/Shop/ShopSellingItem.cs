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

    public Minion Minion { get; internal set; } = null!;

    public override void _Ready()
    {
        _iconTextureRect.Texture = Minion.Sprite;
        _nameLabel.Text = Minion.FriendlyName;
        _buyButton.Text = $"${Minion.Price}";

        // Subscribe 
        var d1 = _buyButton.PressedAsObservable().Subscribe(this, (_, t) => t.OnPressedBuyButton());
        var playerState = (PlayerState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        var d2 = playerState.Money.Subscribe(OnChangedPlayerMoney);

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
        if (money < Minion.Price)
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
        Main.Shop.BuyItem(Minion);
    }

    private void ShowToolTip()
    {
        if (_isSoldOut)
        {
            return;
        }

        var text = $"{Minion.FriendlyName}\n";
        text += $"Tier: {Minion.Tier}\n";
        text += $"Faction: ${Minion.Faction}\n";
        text += $"{Minion.Description}\n";

        ToolTipToast.Text = text;
        ToolTipToast.Show();
    }
}