using System;
using System.Collections.Generic;
using fms.Effect;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
///     Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D, IEffectSolver
{
    /// <summary>
    ///     武器の Id
    ///     Note: Minion から勝手に代入されます
    /// </summary>
    [Export]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     現在の武器の Level
    ///     Note: Minion から勝手に代入されます
    /// </summary>
    [Export(PropertyHint.Range, "1,5")]
    public uint Level { get; set; } = 1;

    /// <summary>
    ///     武器の Cooldown にかかるフレーム数 (ベース値)
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    public uint BaseCoolDownFrame { get; private set; } = 10;

    /// <summary>
    ///     Tree に入った時に自動で Start するかどうか
    /// </summary>
    [Export]
    private bool _autostart;


    
    [ExportGroup("Internal Reference")]
    [Export]
    private FrameTimer _frameTimer = null!;

    private readonly ReactiveProperty<float> _coolDownReduceRateRp = new(0f);
    private readonly List<EffectBase> _effects = new();

    /// <summary>
    ///     Effect の解決後の Cooldown のフレーム数
    /// </summary>
    public uint SolvedCoolDownFrame
    {
        get
        {
            var coolDown = (uint)Mathf.Floor(BaseCoolDownFrame * (1f - _coolDownReduceRateRp.Value));
            return Math.Max(coolDown, 1u);
        }
    }

    /// <summary>
    ///     次の攻撃までの残りフレーム
    /// </summary>
    public ReadOnlyReactiveProperty<int> CoolDownLeft => _frameTimer.FrameLeft;

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            var d1 = _frameTimer.TimeOut.Subscribe(this, (_, state) => { state.DoAttack(state.Level); });
            Disposable.Combine(d1, _coolDownReduceRateRp).AddTo(this);
        }
        else if (what == NotificationReady)
        {
            if (_autostart)
            {
                StartAttack();
            }
            else
            {
                StopAttack();
            }
        }
    }

    public void StartAttack()
    {
        _frameTimer.WaitFrame = SolvedCoolDownFrame;
        _frameTimer.Start();
    }

    public void StopAttack()
    {
        _frameTimer.Stop();
    }

    private protected virtual void DoAttack(uint level)
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
            
            if (effect is AddManaRegeneration addManaRegeneration)
            {
                // ここで何かしらの処理をする
                ManaRegenerationInterval += addManaRegeneration.Interval;
                ManaRegenerationValue += addManaRegeneration.Value;
            }
        }

        OnSolveEffect(_effects);
        _effects.Clear();
    }

    
    private int ManaRegenerationInterval = 0;
    private int ManaRegenerationValue = 0;
    public int Mana = 0;
    private int timeCounter = 0;
    public override void _Process(double delta)
    {
        if (ManaRegenerationInterval == 0 || ManaRegenerationValue == 0)
        {
            return;
        }
        
        timeCounter++;
        if (timeCounter > ManaRegenerationInterval)
        {
            Mana += ManaRegenerationValue;
            timeCounter = 0;
        }
    }
}