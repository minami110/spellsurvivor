using System.Collections.Generic;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
///     Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D, IEffectSolver
{
    [Export]
    private FrameTimer _frameTimer = null!;

    private readonly ReactiveProperty<int> _coolDownLeft = new(1);
    private readonly ReactiveProperty<float> _coolDownReduceRateRp = new(0f);
    private readonly List<EffectBase> _effects = new();

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
                state._frameTimer.WaitFrame = state.CoolDown;
                state._frameTimer.Start();
            }
            else
            {
                state._frameTimer.Stop();
            }
        });

        var d2 = _frameTimer.TimeOut.Subscribe(this, (_, state) => { state.DoAttack(); });

        // Observable の初期化
        var d3 = _coolDownLeft;
        var d5 = _coolDownReduceRateRp;

        Disposable.Combine(d1, d2, d3, d5).AddTo(this);
    }

    private protected virtual void DoAttack()
    {
    }

    private protected virtual void OnSolveEffect(IReadOnlyList<EffectBase> effects)
    {
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