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
            .Subscribe(UpdateLevelUi)
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
            .Subscribe(x => { Main.Shop.Locked = x; })
            .AddTo(this);

        // Player とのバインド
        _player.State.Money.ChangedCurrentValue
            .Subscribe(OnChangedPlayerMoney)
            .AddTo(this);
    }

    private void OnChangedPlayerMoney(uint money)
    {
        // Update the player money text
        GetNode<Label>("%PlayerMoney").Text = $"{money.ToString()}";

        // お金がかかるボタンの Enable / Disable の切り替えを行う
        var rerollButton = GetNode<Button>("%RerollButton");
        rerollButton.Disabled = money < Main.Shop.Config.RerollCost;
        var upgradeButton = GetNode<Button>("%UpgradeButton");
        upgradeButton.Disabled = money < Main.Shop.Config.UpgradeCost;
        var unlockSlotButton0 = GetNode<Button>("%UnlockSlotButton0");
        unlockSlotButton0.Disabled = money < Main.Shop.Config.AddSlotCost;
        var unlockSlotButton1 = GetNode<Button>("%UnlockSlotButton1");
        unlockSlotButton1.Disabled = money < Main.Shop.Config.AddSlotCost;
    }

    private void UpdateLevelUi(uint shopLevel)
    {
        // Update the level text
        GetNode<Label>("%Level").Text = $"Lv {shopLevel.ToString()}";
        // Update odds labels
        UpdateOddsLabels(shopLevel);
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
        }
    }
}