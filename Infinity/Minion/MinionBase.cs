using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

/// <summary>
///     Minion のベースクラス
/// </summary>
public partial class MinionBase : Node2D, IEffectSolver
{
    private readonly ReactiveProperty<int> _coolDownLeft = new(1);
    private readonly List<EffectBase> _effects = new();
    private readonly ReactiveProperty<int> _levelRp = new(1);
    private readonly ReactiveProperty<int> _maxCoolDown = new(1);

    /// <summary>
    ///     Gets the level of this minion. (ReactiveProperty)
    /// </summary>
    public ReadOnlyReactiveProperty<int> Level => _levelRp;

    /// <summary>
    ///     Gets the maximum level of this minion.
    /// </summary>
    public virtual int MaxLevel => 5;

    /// <summary>
    /// </summary>
    private protected virtual int BaseCoolDownFrame => 1;

    /// <summary>
    /// </summary>
    public bool IsMaxLevel => _levelRp.Value >= MaxLevel;

    /// <summary>
    /// </summary>
    public ShopItemSettings ItemSettings { get; set; } = null!;


    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<int> MaxCoolDown => _maxCoolDown;

    /// <summary>
    ///     次の攻撃までの残りフレーム
    /// </summary>
    public ReadOnlyReactiveProperty<int> CoolDownLeft => _coolDownLeft;

    public override void _Ready()
    {
        var d1 = Main.GameMode.WaveStarted.Subscribe(this, (_, t) => t.StartCoolDownTimer());
        var d2 = Main.GameMode.WaveEnded.Subscribe(this, (_, t) => t.StopCoolDownTimer());

        // Observable の初期化
        var d3 = _coolDownLeft;
        var d4 = _levelRp;

        Disposable.Combine(d1, d2, d3, d4).AddTo(this);

        // ToDo:
        _maxCoolDown.Value = BaseCoolDownFrame;
    }

    private protected virtual void DoAttack()
    {
    }

    private protected virtual void OnSolveEffect(List<EffectBase> effects)
    {
    }

    private async void StartCoolDownTimer()
    {
        _coolDownLeft.Value = _maxCoolDown.Value;

        while (true)
        {
            await this.BeginOfProcessAsync();
            _coolDownLeft.Value -= 1;
            if (_coolDownLeft.Value > 0)
            {
                continue;
            }

            DoAttack();
            _coolDownLeft.Value = _maxCoolDown.Value;
        }
    }

    private void StopCoolDownTimer()
    {
    }

    public virtual void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
    }

    public void SolveEffect()
    {
        foreach (var effect in _effects)
            // レベルが上がるエフェクト
            if (!IsMaxLevel && effect is AddLevelEffect addLevelEffect)
            {
                var newLevel = _levelRp.Value + (int)addLevelEffect.Value;
                _levelRp.Value = Mathf.Clamp(newLevel, 0, MaxLevel);
            }

        OnSolveEffect(_effects);
        _effects.Clear();
    }
}