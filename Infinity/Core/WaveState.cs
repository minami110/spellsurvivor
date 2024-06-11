using Godot;
using R3;

namespace fms;

public enum WavePhase
{
    Init,
    Shop,
    Battle,
    Battleresult
}

public partial class WaveState : Node
{
    public enum Signal
    {
        GamemodeStart,
        PlayerAcceptedBattleResult,
        PlayerAcceptedShop
    }

    private readonly ReactiveProperty<double> _battlePhaseTimeLeft = new();

    private readonly ReactiveProperty<WavePhase> _phaseRp = new(WavePhase.Init);
    private readonly ReactiveProperty<int> _waveRp = new(-1);
    public required BattleWaveConfig Config { get; init; }

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<int> Wave => _waveRp;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<WavePhase> Phase => _phaseRp;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<double> BattlePhaseTimeLeft => _battlePhaseTimeLeft;

    public BattleWaveConfigRaw CurrentWaveConfig => Config.Waves[_waveRp.Value];

    public override void _EnterTree()
    {
        // Set Name (for debugging)
        Name = nameof(ShopState);
    }

    public override void _Process(double delta)
    {
        if (_phaseRp.Value != WavePhase.Battle)
        {
            return;
        }

        _battlePhaseTimeLeft.Value -= delta;
        if (_battlePhaseTimeLeft.Value <= 0)
        {
            _battlePhaseTimeLeft.Value = 0;
            _phaseRp.Value = WavePhase.Battleresult;
        }
    }

    public override void _ExitTree()
    {
        _phaseRp.Dispose();
        _battlePhaseTimeLeft.Dispose();
        _waveRp.Dispose();
    }

    public void SendSignal(Signal signal)
    {
        if (signal == Signal.GamemodeStart)
        {
            if (_phaseRp.Value == WavePhase.Init)
            {
                _phaseRp.Value = WavePhase.Shop;
            }
        }
        else if (signal == Signal.PlayerAcceptedBattleResult)
        {
            if (_phaseRp.Value == WavePhase.Battleresult)
            {
                _phaseRp.Value = WavePhase.Shop;
            }
        }
        else if (signal == Signal.PlayerAcceptedShop)
        {
            if (_phaseRp.Value == WavePhase.Shop)
            {
                // Wave を次に進める
                // Note: Settings が足りない場合はループさせるので永遠に終わらない
                var nextWaveIndex = _waveRp.Value + 1;
                if (nextWaveIndex >= Config.WaveCount)
                {
                    nextWaveIndex = 0;
                }

                _waveRp.Value = nextWaveIndex;

                var currentConfig = Config.Waves[nextWaveIndex];

                // 制限時間を決定する
                _battlePhaseTimeLeft.Value = currentConfig.WaveTimeSeconds;

                // バトルフェーズに移行
                _phaseRp.Value = WavePhase.Battle;
            }
        }
    }
}