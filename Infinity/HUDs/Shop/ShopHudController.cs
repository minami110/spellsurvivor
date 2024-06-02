using Godot;
using R3;

namespace fms;

public partial class ShopHudController : Node
{
    [Export]
    private Control _container = null!;

    [Export]
    private PackedScene _shopItemPackedScene = null!;

    [Export]
    private PackedScene _shopOwnItemPackedScene = null!;

    [Export]
    private Control _shopItemSpawnParent = null!;

    [Export]
    private Control _equipItemSpawnParent = null!;

    [Export]
    private Button _upgradeButton = null!;

    [Export]
    private Button _rerollButton = null!;

    [Export]
    private Button _lockButton = null!;

    [Export]
    private Button _quitShopButton = null!;

    [Export]
    private Label _playerMoneyLabel = null!;

    [Export]
    private Label _shopLevelLabel = null!;

    public override void _Ready()
    {
        // ボタンの名前を更新
        _upgradeButton.Text = $"Upgrade (${Main.ShopState.Config.UpgradeCost})";
        _rerollButton.Text = $"Reroll (${Main.ShopState.Config.RerollCost})";

        // ボタンのバインドを更新
        var d01 = _rerollButton.PressedAsObservable().Subscribe(_ => { Main.ShopState.RefreshInStoreMinions(); });
        var d02 = _upgradeButton.PressedAsObservable().Subscribe(_ => { Main.ShopState.UpgradeShopLevel(); });
        var d03 = _quitShopButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PLAYER_ACCEPTED_SHOP);
        });
        var d04 = _lockButton.PressedAsObservable().Subscribe(_ =>
        {
            if (_lockButton.Text == "Lock")
            {
                _lockButton.Text = "Unlock";
                foreach (var m in Main.ShopState.InStoreMinions)
                {
                    m.Lock();
                }
            }
            else
            {
                _lockButton.Text = "Lock";
                foreach (var m in Main.ShopState.InStoreMinions)
                {
                    m.Unlock();
                }
            }
        });

        // Player Money の変更を監視
        var d4 = Main.PlayerState.Money.Subscribe(this, (x, state) => state._playerMoneyLabel.Text = $"$ {x}");

        // ShopState
        var d5 = Main.ShopState.Level.Subscribe(x => { _shopLevelLabel.Text = $"Lv.{Main.ShopState.Level}"; });
        var d6 = Main.ShopState.InStoreMinionsUpdated.Subscribe(OnInStoreMinionsUpdated);

        // WaveState
        var d7 = Main.WaveState.Phase.Subscribe(this, (x, state) =>
        {
            if (x == WavePhase.SHOP)
            {
                state._container.Show();
            }
            else
            {
                state._container.Hide();
            }
        });

        Disposable.Combine(d01, d02, d03, d04, d4, d5, d6, d7).AddTo(this);
    }

    private void OnInStoreMinionsUpdated(Unit _)
    {
        // すでに生成している ShopOwnItem を削除 する
        foreach (var old in _shopItemSpawnParent.GetChildren())
        {
            old.QueueFree();
        }

        foreach (var item in Main.ShopState.InStoreMinions)
        {
            var node = _shopItemPackedScene.Instantiate<ShopSellingItem>();
            {
                node.ShopItemSettings = item;
            }
            _shopItemSpawnParent.AddChild(node);
        }
    }
}