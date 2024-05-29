using fms;
using Godot;
using R3;

/// <summary>
///     Shop で販売しているアイテム の HUD, Shop により自動で生成される
/// </summary>
public partial class ShopSellingItem : VBoxContainer
{
    [Export]
    public ShopItemSettings ShopItemSettings { get; set; } = null!;

    [ExportGroup("Internal Reference")]
    [Export]
    private TextureRect _iconTextureRect = null!;

    [Export]
    private Label _nameLabel = null!;

    [Export]
    private Button _buyButton = null!;

    public override void _Ready()
    {
        _iconTextureRect.Texture = ShopItemSettings.Icon;
        _nameLabel.Text = ShopItemSettings.Name;
        _buyButton.Text = $"${ShopItemSettings.Price}";

        // Subscribe 
        var d1 = _buyButton.PressedAsObservable().Subscribe(this, (_, t) => t.OnPressedBuyButton());
        var d2 = Main.PlayerState.Money.Subscribe(OnChangedPlayerMoney);
        Disposable.Combine(d1, d2).AddTo(this);
    }

    private void OnChangedPlayerMoney(int money)
    {
        if (money < ShopItemSettings.Price)
        {
            _buyButton.Modulate = new Color(1, 0, 0);
            _buyButton.Disabled = true;
        }
        else
        {
            _buyButton.Modulate = new Color(0, 1, 0);
            _buyButton.Disabled = false;
        }
    }

    private void OnPressedBuyButton()
    {
        // GameMode に通知する
        Main.GameMode.BuyItem(ShopItemSettings);

        //この Shop Item を無効化する
        _iconTextureRect.Hide();
        _nameLabel.Hide();
        _buyButton.Hide();
    }
}