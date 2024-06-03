using System;
using R3;

namespace fms;

public enum WavePhase
{
    Init,
    SHOP,
    BATTLE,
    BATTLERESULT
}

public sealed class WaveState : IDisposable
{
    public enum Signal
    {
        GAMEMODE_START,
        PLAYER_ACCEPTED_BATTLE_RESULT,
        PLAYER_ACCEPTED_SHOP
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

    internal void _Process(double delta)
    {
        if (_phaseRp.Value != WavePhase.BATTLE)
        {
            return;
        }

        _battlePhaseTimeLeft.Value -= delta;
        if (_battlePhaseTimeLeft.Value <= 0)
        {
            _battlePhaseTimeLeft.Value = 0;
            _phaseRp.Value = WavePhase.BATTLERESULT;
        }
    }

    public void SendSignal(Signal signal)
    {
        if (signal == Signal.GAMEMODE_START)
        {
            if (_phaseRp.Value == WavePhase.Init)
            {
                _phaseRp.Value = WavePhase.SHOP;
            }
        }
        else if (signal == Signal.PLAYER_ACCEPTED_BATTLE_RESULT)
        {
            if (_phaseRp.Value == WavePhase.BATTLERESULT)
            {
                _phaseRp.Value = WavePhase.SHOP;
            }
        }
        else if (signal == Signal.PLAYER_ACCEPTED_SHOP)
        {
            if (_phaseRp.Value == WavePhase.SHOP)
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
                _phaseRp.Value = WavePhase.BATTLE;
            }
        }
    }

    void IDisposable.Dispose()
    {
        _phaseRp.Dispose();
        _battlePhaseTimeLeft.Dispose();
        _waveRp.Dispose();
    }
}