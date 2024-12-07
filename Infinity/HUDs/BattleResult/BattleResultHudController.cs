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
                var reward = Main.WaveState.CurrentWaveConfig.Reward;
                var playerMoney = playerState.Money.CurrentValue;

                self._titleLabel.Text = $"Wave {Main.WaveState.Wave.CurrentValue} Result";
                self._rewardLabel.Text = $"Money: ${playerMoney + reward} (+${reward})";
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