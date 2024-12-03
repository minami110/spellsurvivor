using Godot;
using R3;

namespace fms.HUD;

public partial class SellingWeaponCardContainer : Control
{
    private IEntity _player = null!;

    public override void _Ready()
    {
        _player = (IEntity)this.GetPlayerNode();

        // Shop との バインド
        // Level
        Main.Shop.Level
            .Subscribe(OnChangedShopLevel)
            .AddTo(this);

        // Upgrade Button
        var upgradeButton = GetNode<Button>("%UpgradeButton");
        upgradeButton.PressedAsObservable()
            .Subscribe(_ => { Main.Shop.UpgradeShopLevel(_player); })
            .AddTo(this);

        // Reroll Button
        var rerollButton = GetNode<Button>("%RerollButton");
        rerollButton.PressedAsObservable()
            .Subscribe(_ => { Main.Shop.RerollWeaponCards(_player); })
            .AddTo(this);

        // Shop Lock Button
        var lockButton = GetNode<LockButton>("%LockButton");
        lockButton.Locked
            .Subscribe(x =>
            {
                Main.Shop.Locked = x;
                GetNode<BaseButton>("%RerollButton").Disabled = x;
            })
            .AddTo(this);

        // Add Slot Button
        var unlockSlotButton0 = GetNode<Button>("%UnlockSlotButton0");
        unlockSlotButton0.PressedAsObservable()
            .Subscribe(_ => { Main.Shop.BuyWeaponCardSlot(_player); })
            .AddTo(this);
        var unlockSlotButton1 = GetNode<Button>("%UnlockSlotButton1");
        unlockSlotButton1.PressedAsObservable()
            .Subscribe(_ => { Main.Shop.BuyWeaponCardSlot(_player); })
            .AddTo(this);
        Main.Shop.CardSlotCount
            .Subscribe(OnChangedCardSlotCount)
            .AddTo(this);

        // Shop で販売中のカードの更新
        Main.Shop.InStoreWeaponCardsUpdated
            .Subscribe(_ =>
            {
                var cards = Main.Shop.InStoreWeaponCards;
                for (var i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    var button = GetNode<SellingWeaponCardButton>($"%WeaponCardButton{i}");
                    button.WeaponCard = card;
                }
            })
            .AddTo(this);

        // 販売中のカードの Tooltip 表示
        for (var i = 0; i < 4; i++)
        {
            var button = GetNode<SellingWeaponCardButton>($"%WeaponCardButton{i}");
            button.RequestShowInfo
                .Subscribe(info =>
                {
                    // Show Tool tip
                    var toast = GetNode<WeaponDescriptionToast>("%WeaponDescriptionToast");
                    toast.Show();
                    toast.Header = (string)info["Title"];
                    toast.Description = (string)info["Description"];
                })
                .AddTo(this);
            button.RequestHideInfo
                .Subscribe(_ =>
                {
                    var toast = GetNode<WeaponDescriptionToast>("%WeaponDescriptionToast");
                    toast.Hide();
                })
                .AddTo(this);
        }

        // ロックされたら Reroll ボタンを Disable にする
        Main.Shop.CardSlotLocked
            .Subscribe(x => { UpdateRerollButtonDisabled(); })
            .AddTo(this);

        // チート系のボタン
        // ToDo: リリースでは削除するような仕組みを考える
        var cheatAddMoneyButton = GetNode<Button>("%CheetAddMoneyButton");
        cheatAddMoneyButton.PressedAsObservable()
            .Subscribe(_ =>
            {
                var player = (IEntity)this.GetPlayerNode();
                player.State.AddMoney(10u);
            })
            .AddTo(this);

        // Player とのバインド
        _player.State.Money.ChangedCurrentValue
            .Subscribe(OnChangedPlayerMoney)
            .AddTo(this);
    }

    private void OnChangedCardSlotCount(uint cardSlotCount)
    {
        switch (cardSlotCount)
        {
            case 4:
            {
                GetNode<Button>("%UnlockSlotButton0").Hide();
                GetNode<Button>("%WeaponCardButton3").Show();
                break;
            }
            case 5:
            {
                GetNode<Button>("%UnlockSlotButton0").Hide();
                GetNode<Button>("%UnlockSlotButton1").Hide();
                GetNode<Button>("%WeaponCardButton3").Show();
                GetNode<Button>("%WeaponCardButton4").Show();
                break;
            }
        }
    }

    private void OnChangedPlayerMoney(uint money)
    {
        // Update the player money text
        GetNode<Label>("%PlayerMoney").Text = $"{money.ToString()}";

        // お金がかかるボタンの Enable / Disable の切り替えを行う
        UpdateRerollButtonDisabled();
        var upgradeButton = GetNode<FmsButton>("%UpgradeButton");
        upgradeButton.Disabled = money < Main.Shop.Config.UpgradeCost;
        var unlockSlotButton0 = GetNode<FmsButton>("%UnlockSlotButton0");
        unlockSlotButton0.Disabled = money < Main.Shop.Config.AddSlotCost;
        var unlockSlotButton1 = GetNode<FmsButton>("%UnlockSlotButton1");
        unlockSlotButton1.Disabled = money < Main.Shop.Config.AddSlotCost;
    }

    private void OnChangedShopLevel(uint shopLevel)
    {
        // Update the level text
        GetNode<Label>("%Level").Text = $"Lv {shopLevel.ToString()}";
        // Update odds labels
        UpdateOddsLabels(shopLevel);

        if (shopLevel == Constant.SHOP_MAX_LEVEL)
        {
            var upgradeButton = GetNode<ShopActionButton>("%UpgradeButton");
            upgradeButton.Title = "Max Level";
            upgradeButton.Cost = 0u;
            upgradeButton.Disabled = true;
        }
    }

    private void UpdateOddsLabels(uint shopLevel)
    {
        // Update the tier odds labels
        var odds = Main.Shop.Config.Odds;
        var tierCount = Constant.MINION_MAX_TIER;

        // Note: Shop Level 1 なら 0, 1, 2, 3, 4 が該当する
        var startIndex = (shopLevel - 1) * tierCount;

        // Note: odds は 25%, 75% のばあい, 0.25, 1.0 という値が入っているので
        // UI 用に 引いていく処理を行っている
        var remaining = 1.0f;
        for (var i = 0; i < tierCount; i++)
        {
            var label = GetNode<Label>($"%TierRate{i}");
            var rate = odds[startIndex + i];
            if (rate > 0f)
            {
                var r = remaining * rate;
                label.Text = $"{r * 100f:0}%";
                remaining -= r;
            }
            else
            {
                label.Text = "0%";
            }

            label.Modulate = i switch
            {
                0 => FmsColors.TierCommon,
                1 => FmsColors.TierUncommon,
                2 => FmsColors.TierRare,
                3 => FmsColors.TierEpic,
                4 => FmsColors.TierLegendary,
                _ => label.Modulate
            };
            label.Modulate *= 1.2f;
        }
    }

    private void UpdateRerollButtonDisabled()
    {
        var button = GetNode<FmsButton>("%RerollButton");
        var disabled = Main.Shop.CardSlotLocked.CurrentValue ||
                       Main.Shop.Config.RerollCost > _player.State.Money.CurrentValue;
        button.Disabled = disabled;
    }
}