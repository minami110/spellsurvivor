using Godot;
using R3;

namespace fms.HUD;

public partial class ShopHudController : Node
{
    [Export]
    private Control _container = null!;

    [Export]
    private PackedScene _shopOwnItemPackedScene = null!;

    [Export]
    private Control _equipItemSpawnParent = null!;

    [Export]
    private Button _quitShopButton = null!;

    public override void _Ready()
    {
        // ボタンの名前を更新
        // ボタンのバインドを更新
        var d02 = _quitShopButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedShop);
        });
        // Player Inventory
        var playerNode = this.GetPlayerNode();
        var d20 = playerNode.ChildOrderChangedAsObservable().Subscribe(OnEquippedMinionChanged);

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

        Disposable.Combine(d02, d20, d40).AddTo(this);
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
        foreach (var n in playerNode.GetChildren())
        {
            if (n is not WeaponCard weaponCard)
            {
                continue;
            }

            var node = _shopOwnItemPackedScene.Instantiate<ShopOwnItem>();
            {
                node.WeaponCard = weaponCard;
            }
            _equipItemSpawnParent.AddChild(node);
        }
    }
}