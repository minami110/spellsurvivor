using fms;
using Godot;
using R3;

/// <summary>
///     Shop で販売しているアイテム
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
        _buyButton.Text = $"Buy ${ShopItemSettings.Price}";

        // Subscribe 
        var d1 = _buyButton.PressedAsObservable().Subscribe(_ => OnPressedBuyButton());
        var d2 = Main.PlayerState.Money.Subscribe(OnChangedPlayerMoney);
        Disposable.Combine(d1, d2).AddTo(this);
    }

    private void OnChangedPlayerMoney(int money)
    {
        _buyButton.Disabled = money < ShopItemSettings.Price;
    }

    private void OnPressedBuyButton()
    {
        // GameMode に通知する
        Main.GameMode.BuyItem(ShopItemSettings);

        // ToDo: この Shop Item を無効化する
    }
}