using System;
using fms;
using Godot;
using R3;

public partial class ShopManager : Node
{
    [Export]
    private int _slotCount = 3;

    [Export]
    private int _rerollPrice = 1;

    [Export]
    private ShopItemSettings[] _itemPool = Array.Empty<ShopItemSettings>();

    [Export]
    private PackedScene _shopItemPackedScene = null!;

    [Export]
    private PackedScene _shopOwnItemPackedScene = null!;

    [Export]
    private Control _shopItemSpawnParent = null!;

    [Export]
    private Control _equipItemSpawnParent = null!;

    [Export]
    private BaseButton _rerollButton = null!;

    [Export]
    private Label _playerMoneyLabel = null!;

    public override void _Ready()
    {
        var d1 = _rerollButton.PressedAsObservable().Subscribe(_ => OnPressedReRollButton());
        var d2 = Main.PlayerState.Money.Subscribe(x => _playerMoneyLabel.Text = $"$ {x}");
        var d3 = Main.GameMode.EquippedItem.Subscribe(OnPlayerEquippedItem);

        Disposable.Combine(d1, d2, d3).AddTo(this);
    }

    public void Reroll()
    {
        // すでに生成している ShopItem を削除 する
        foreach (var node in _shopItemSpawnParent.GetChildren()) node.QueueFree();

        // ShopItem を生成する
        InstantiateShopItems();
    }

    private void InstantiateShopItems()
    {
        // ToDo: すでに MaxLevel に達した アイテムは Pool から取り除く

        var arrayLength = _itemPool.Length;
        var rand = new Random();

        for (var i = 0; i < _slotCount; i++)
        {
            // Pick random index
            var id = rand.Next(arrayLength);
            var settings = _itemPool[id];

            // Spawn ShopItem
            var node = _shopItemPackedScene.Instantiate<ShopSellingItem>();
            {
                node.ShopItemSettings = settings;
            }
            _shopItemSpawnParent.AddChild(node);
        }
    }

    private void OnPlayerEquippedItem(ShopItemSettings itemSetting)
    {
        var node = _shopOwnItemPackedScene.Instantiate<ShopOwnItem>();
        {
            node.ItemSettings = itemSetting;
        }
        _equipItemSpawnParent.AddChild(node);
    }

    private void OnPressedReRollButton()
    {
        // ToDo: お金チェック
        Reroll();
    }
}