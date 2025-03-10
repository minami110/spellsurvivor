using Godot;
using R3;

namespace fms;

internal partial class BattleResultHudController : Node
{
    [Export]
    private Button _acceptButton = null!;

    [Export]
    private Label _titleLabel = null!;

    [Export]
    private Label _rewardLabel = null!;

    public override void _Ready()
    {
        GetNode<Control>("%RootPanel").Hide();

        var ws = Main.WaveState;
        var d1 = ws.Phase.Subscribe(this, (p, self) =>
        {
            if (p == WavePhase.Battleresult)
            {
                var playerState = (EntityState)self.GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
                self._titleLabel.Text = $"Wave {Main.WaveState.Wave.CurrentValue} Result";

                // 実際の計算はこの後の Main のフェーズで行われる
                var prevMoney = playerState.Money.CurrentValue;
                var willMoney = prevMoney;
                var goldNuggetPlus = 0u;
                var interestPlus = 0u;

                // 1 .現在の GoldNugget を換金する
                // https://scrapbox.io/FUMOSurvivor/%E3%82%B4%E3%83%BC%E3%83%AB%E3%83%89%E3%83%8A%E3%82%B2%E3%83%83%E3%83%88%E3%83%86%E3%83%BC%E3%83%96%E3%83%AB
                // TODO: 仮の処理, 10 GoldNugget を 1 Money に換金して減らす
                var goldNuggetShopLevel = Main.GoldNuggetShop.ShopLevel;
                var nextGoldNugget = GoldNuggetShop.GetGoldNuggetAmount(goldNuggetShopLevel);
                var prevGoldNugget = playerState.GoldNugget.CurrentValue;
                while (nextGoldNugget <= prevGoldNugget)
                {
                    willMoney += 1;
                    goldNuggetPlus += 1;
                    prevGoldNugget -= nextGoldNugget;
                    goldNuggetShopLevel += 1;
                    nextGoldNugget = GoldNuggetShop.GetGoldNuggetAmount(goldNuggetShopLevel);
                }

                // 2. 現在の所持金から利子を発生させる
                // 現在の所持金 の 10% の切り捨て, 最大 5 まで
                // 32 => 3, 48 => 4, 96 => 5
                interestPlus = (uint)Mathf.Min(Mathf.Floor(willMoney * 0.1f), 5.0);
                willMoney += interestPlus;

                // 3. Wave 報酬を与える
                var wavePlus = Main.WaveState.CurrentWaveConfig.Reward;
                willMoney += (uint)wavePlus;


                var rewardText = "";
                rewardText = $"お金: {prevMoney} => {willMoney}\n";
                rewardText += $"  ゴールドナゲット: {goldNuggetPlus}\n";
                rewardText += $"  利子: {interestPlus}\n";
                rewardText += $"  Wave報酬: {wavePlus}";
                self._rewardLabel.Text = rewardText;
                self.GetNode<Control>("%RootPanel").Show();
            }
            else
            {
                self.GetNode<Control>("%RootPanel").Hide();
            }
        });

        var d2 = _acceptButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedBattleResult);
        });

        Disposable.Combine(d1, d2).AddTo(this);
    }
}