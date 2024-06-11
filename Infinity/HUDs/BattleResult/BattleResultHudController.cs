using Godot;
using R3;

namespace fms;

internal partial class BattleResultHudController : Node
{
    [Export]
    private Control _rootControl = null!;

    [Export]
    private Button _acceptButton = null!;

    [Export]
    private Label _titleLabel = null!;

    [Export]
    private Label _rewardLabel = null!;

    public override void _Ready()
    {
        _rootControl.Hide();

        var ws = Main.WaveState;
        var d1 = ws.Phase.Subscribe(this, (p, state) =>
        {
            if (p == WavePhase.Battleresult)
            {
                var playerState = (PlayerState)GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayerState);
                var reward = Main.WaveState.CurrentWaveConfig.Reward;
                var playerMoney = playerState.Money.CurrentValue;

                state._titleLabel.Text = $"Wave {Main.WaveState.Wave.CurrentValue} Result";
                state._rewardLabel.Text = $"Money: ${playerMoney + reward} (+${reward})";
                state._rootControl.Show();
            }
            else
            {
                state._rootControl.Hide();
            }
        });

        var d2 = _acceptButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedBattleResult);
        });

        Disposable.Combine(d1, d2).AddTo(this);
    }
}