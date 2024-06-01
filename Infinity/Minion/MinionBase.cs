using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using fms.Faction;
using Godot;
using R3;

namespace fms.Minion;

/// <summary>
///     Minion のベースクラス
/// </summary>
public partial class MinionBase : Node2D, IEffectSolver
{
    private const int _MIN_LEVEL = 1;

    private readonly ReactiveProperty<int> _coolDownLeft = new(1);
    private readonly ReactiveProperty<float> _coolDownReduceRateRp = new(0f);
    private readonly List<EffectBase> _effects = new();
    private readonly ReactiveProperty<int> _levelRp = new(_MIN_LEVEL);
    private CancellationTokenSource? _runningFrameTimerCts;

    /// <summary>
    ///     Gets the level of this minion. (ReactiveProperty)
    /// </summary>
    public ReadOnlyReactiveProperty<int> Level => _levelRp;

    /// <summary>
    ///     Gets the cool down reduce rate of this minion. unit is %.
    /// </summary>
    public ReadOnlyReactiveProperty<float> CoolDownReduceRate => _coolDownReduceRateRp;

    public int CoolDown
    {
        get
        {
            var coolDown = BaseCoolDownFrame;
            var reduceRate = _coolDownReduceRateRp.Value * coolDown;
            return Mathf.Max(coolDown - (int)reduceRate, 1);
        }
    }

    /// <summary>
    ///     Gets the maximum level of this minion.
    /// </summary>
    public virtual int MaxLevel => 5;

    /// <summary>
    ///     Minion のベースの攻撃間隔
    /// </summary>
    private protected virtual int BaseCoolDownFrame => 1;

    /// <summary>
    ///     Minion が所属する Faction (シナジー) を取得
    /// </summary>
    public virtual FactionBase[] Factions { get; } = Array.Empty<FactionBase>();

    /// <summary>
    /// </summary>
    public bool IsMaxLevel => _levelRp.Value >= MaxLevel;

    /// <summary>
    /// </summary>
    public MinionCoreData MinionCoreData { get; set; } = null!;

    /// <summary>
    ///     次の攻撃までの残りフレーム
    /// </summary>
    public ReadOnlyReactiveProperty<int> CoolDownLeft => _coolDownLeft;

    public override void _EnterTree()
    {
        var d1 = Main.Instance.WaveStarted.Subscribe(this, (_, t) => t.StartCoolDownTimer());
        var d2 = Main.Instance.WaveEnded.Subscribe(this, (_, t) => t.StopCoolDownTimer());

        // Observable の初期化
        var d3 = _coolDownLeft;
        var d4 = _levelRp;
        var d5 = _coolDownReduceRateRp;

        Disposable.Combine(d1, d2, d3, d4, d5).AddTo(this);

        // Tree から抜けるときにタイマーを止める
        TreeExiting += StopCoolDownTimer;
    }

    public bool IsFaction<T>() where T : FactionBase
    {
        return Factions.Any(f => f.GetType() == typeof(T));
    }

    private protected virtual void DoAttack()
    {
    }

    private protected virtual void OnSolveEffect(IReadOnlyList<EffectBase> effects)
    {
    }

    private async void StartCoolDownTimer()
    {
        if (_runningFrameTimerCts is not null)
        {
            // Already Running
            return;
        }

        _runningFrameTimerCts = new CancellationTokenSource();
        var token = _runningFrameTimerCts.Token;
        _coolDownLeft.Value = CoolDown;

        var tree = GetTree();

        while (!token.IsCancellationRequested)
        {
            await this.BeginOfProcessAsync();

            if (tree.Paused)
            {
                continue;
            }

            _coolDownLeft.Value -= 1;
            if (_coolDownLeft.Value > 0)
            {
                continue;
            }

            DoAttack();
            _coolDownLeft.Value = CoolDown;
        }
    }

    private void StopCoolDownTimer()
    {
        _runningFrameTimerCts?.Cancel();
        _runningFrameTimerCts = null;
    }

    public virtual void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
    }

    public void SolveEffect()
    {
        foreach (var effect in _effects)
        {
            if (!IsMaxLevel && effect is AddLevelEffect addLevelEffect)
            {
                var newLevel = _levelRp.Value + addLevelEffect.Value;
                _levelRp.Value = Mathf.Clamp(newLevel, _MIN_LEVEL, MaxLevel);
            }
            else if (effect is ReduceCoolDownRate reduceCoolDownRate)
            {
                var newRate = _coolDownReduceRateRp.Value + reduceCoolDownRate.Value;
                _coolDownReduceRateRp.Value = Mathf.Clamp(newRate, 0f, 1f);
            }
        }

        OnSolveEffect(_effects);
        _effects.Clear();
    }
}