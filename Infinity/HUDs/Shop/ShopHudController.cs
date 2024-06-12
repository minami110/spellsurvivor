using Godot;
using R3;

namespace fms;

public partial class ShopHudController : Node
{
    [Export]
    private Control _container = null!;

    [Export]
    private PackedScene _instoreItemPackedScene = null!;

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
    private Button _addSlotButton = null!;

    [Export]
    private Button _quitShopButton = null!;

    [Export]
    private Label _playerMoneyLabel = null!;

    [Export]
    private Label _shopLevelLabel = null!;

    public override void _Ready()
    {
        // ボタンの名前を更新
        _upgradeButton.Text = $"Upgrade (${Main.Shop.Config.UpgradeCost})";
        _rerollButton.Text = $"Reroll (${Main.Shop.Config.RerollCost})";
        _addSlotButton.Text = $"AddSlot (${Main.Shop.Config.AddSlotCost})";
        _lockButton.Text = Main.Shop.IsLocked ? "Unlock" : "Lock";

        // ボタンのバインドを更新
        var d00 = _rerollButton.PressedAsObservable().Subscribe(_ => { Main.Shop.RefreshInStoreMinions(); });
        var d01 = _upgradeButton.PressedAsObservable().Subscribe(_ => { Main.Shop.UpgradeShopLevel(); });
        var d02 = _quitShopButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedShop);
        });
        var d03 = _lockButton.PressedAsObservable().Subscribe(_ =>
        {
            if (Main.Shop.IsLocked)
            {
                _lockButton.Text = "Lock";
                Main.Shop.IsLocked = false;
            }
            else
            {
                _lockButton.Text = "Unlock";
                Main.Shop.IsLocked = true;
            }
        });
        var d04 = _addSlotButton.PressedAsObservable().Subscribe(_ => { Main.Shop.AddItemSlot(); });


        // Player Money の変更を監視
        var playerState = (PlayerState)GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayerState);
        var d10 = playerState.Money.Subscribe(this, (x, state) => state._playerMoneyLabel.Text = $"$ {x}");

        // Player Inventory
        var playerNode = this.GetPlayerNode();
        var d20 = playerNode.ChildOrderChangedAsObservable().Subscribe(OnEquippedMinionChanged);

        // ShopState
        var d30 = Main.Shop.Level.Subscribe(x => { _shopLevelLabel.Text = $"Lv.{Main.Shop.Level}"; });
        var d31 = Main.Shop.InStoreMinionsUpdated.Subscribe(OnInStoreMinionsUpdated);

        // WaveState
        var d40 = Main.WaveState.Phase.Subscribe(this, (x, state) =>
        {
            if (x == WavePhase.Shop)
            {
                state._container.Show();
            }
            else
            {
                state._container.Hide();
            }
        });

        Disposable.Combine(d00, d01, d02, d03, d04, d10, d20, d30, d31, d40).AddTo(this);
    }

    private void OnEquippedMinionChanged(Unit _)
    {
        // すでに生成している ShopOwnItem を削除 する
        foreach (var old in _equipItemSpawnParent.GetChildren())
        {
            old.QueueFree();
        }

        // Player が所有している Minion を取得する
        var playerNode = this.GetPlayerNode();
        var minions = playerNode.FindChildren("*", nameof(Minion), false, false);
        foreach (var minion in minions)
        {
            var node = _shopOwnItemPackedScene.Instantiate<ShopOwnItem>();
            {
                node.Minion = (Minion)minion;
            }
            _equipItemSpawnParent.AddChild(node);
        }
    }

    private void OnInStoreMinionsUpdated(Unit _)
    {
        // すでに生成している ShopOwnItem を削除 する
        foreach (var old in _shopItemSpawnParent.GetChildren())
        {
            old.QueueFree();
        }

        foreach (var item in Main.Shop.InStoreMinions)
        {
            var node = _instoreItemPackedScene.Instantiate<ShopSellingItem>();
            {
                node.Minion = item;
            }
            _shopItemSpawnParent.AddChild(node);
        }
    }
}