using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using fms.Faction;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
///     Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D, IEffectSolver
{
    private readonly ReactiveProperty<int> _coolDownLeft = new(1);
    private readonly ReactiveProperty<float> _coolDownReduceRateRp = new(0f);
    private readonly List<EffectBase> _effects = new();
    private CancellationTokenSource? _runningFrameTimerCts;

    public string Id => CoreData.Id;

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
    ///     ベースの攻撃間隔 (単位: Frame)
    /// </summary>
    private protected virtual int BaseCoolDownFrame => 1;

    /// <summary>
    ///     Minion が所属する Faction (シナジー) を取得
    /// </summary>
    public virtual FactionBase[] Factions { get; } = Array.Empty<FactionBase>();

    /// <summary>
    /// </summary>
    public MinionInRuntime CoreData { get; set; } = null!;

    /// <summary>
    ///     次の攻撃までの残りフレーム
    /// </summary>
    public ReadOnlyReactiveProperty<int> CoolDownLeft => _coolDownLeft;

    /// <summary>
    ///     この武器を所有している Minion のレベル
    /// </summary>
    public int MinionLevel => CoreData.Level.CurrentValue;

    public override void _EnterTree()
    {
        var d1 = Main.WaveState.Phase.Subscribe(this, (phase, state) =>
        {
            if (phase == WavePhase.BATTLE)
            {
                state.StartCoolDownTimer();
            }
            else
            {
                state.StopCoolDownTimer();
            }
        });

        // Observable の初期化
        var d3 = _coolDownLeft;
        var d5 = _coolDownReduceRateRp;

        Disposable.Combine(d1, d3, d5).AddTo(this);

        // Tree から抜けるときにタイマーを止める
        TreeExiting += StopCoolDownTimer;
    }

    /// <summary>
    ///     指定された Type の Faction に所属しているかどうか
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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
            if (effect is ReduceCoolDownRate reduceCoolDownRate)
            {
                var newRate = _coolDownReduceRateRp.Value + reduceCoolDownRate.Value;
                _coolDownReduceRateRp.Value = Mathf.Clamp(newRate, 0f, 1f);
            }
        }

        OnSolveEffect(_effects);
        _effects.Clear();
    }
}