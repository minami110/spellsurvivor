using R3;

namespace fms;

public class Shop
{
    private const int _MIN_SHOP_LEVEL = 0;
    private const int _MAX_SHOP_LEVEL = 5;

    private readonly ReactiveProperty<int> _shopLevelRp = new();

    /// <summary>
    ///     Get the current shop level
    /// </summary>
    public ReadOnlyReactiveProperty<int> ShopLevel => _shopLevelRp;

    public void RerollShopItems()
    {
    }

    public void UpgradeShopLevel()
    {
        if (_shopLevelRp.Value >= _MAX_SHOP_LEVEL)
        {
            return;
        }

        _shopLevelRp.Value++;
    }
}