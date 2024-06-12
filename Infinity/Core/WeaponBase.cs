using System;
using System.Collections.Generic;
using fms.Effect;
using fms.Faction;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
///     Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D
{
    /// <summary>
    ///     武器の Cooldown にかかるフレーム数 (ベース値)
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    public uint BaseCoolDownFrame { get; private set; } = 10;

    /// <summary>
    ///     Tree に入った時に自動で Start するかどうか
    /// </summary>
    [ExportGroup("For Debugging")]
    [Export]
    private bool _autostart;

    /// <summary>
    ///     現在の武器の Level
    ///     Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export(PropertyHint.Range, "1,5")]
    public uint Level { get; set; } = 1;

    /// <summary>
    ///     Minion が所属する Faction
    ///     Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export]
    public FactionType Faction { get; set; }

    private readonly ReactiveProperty<float> _coolDownReduceRateRp = new(0f);
    private readonly HashSet<EffectBase> _effects = new();

    private FrameTimer _frameTimer = null!;

    private bool _isDirty;


    private int _manaRegenerationInterval;
    private int _manaRegenerationValue;
    private int _timeCounter;
    public int Mana;

    /// <summary>
    ///     武器の Id
    ///     Note: Minion から勝手に代入されます
    /// </summary>
    public string MinionId { get; set; } = string.Empty;

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
            Name = $"(Weapon) {MinionId}";
            if (!IsInGroup(Constant.GroupNameWeapon))
            {
                AddToGroup(Constant.GroupNameWeapon);
            }
        }
        else if (what == NotificationReady)
        {
            _frameTimer = GetNode<FrameTimer>("FrameTimer");
            var d1 = _frameTimer.TimeOut.Subscribe(this, (_, state) => { state.DoAttack(state.Level); });
            Disposable.Combine(d1, _coolDownReduceRateRp).AddTo(this);

            if (_autostart)
            {
                StartAttack();
            }
            else
            {
                StopAttack();
            }

            // Note: Process を override していないのでここで手動で有効化する
            SetProcess(true);
        }
        else if (what == NotificationProcess)
        {
            SolveEffect();
        }
    }

    public override void _Process(double delta)
    {
        if (_manaRegenerationInterval == 0 || _manaRegenerationValue == 0)
        {
            return;
        }

        _timeCounter++;
        if (_timeCounter > _manaRegenerationInterval)
        {
            Mana += _manaRegenerationValue;
            _timeCounter = 0;
        }
    }

    public virtual void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
        _isDirty = true;
    }

    public bool IsBelongTo(FactionType factionType)
    {
        return Faction.HasFlag(factionType);
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

    private protected virtual void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
    }

    private void SolveEffect()
    {
        if (_effects.Count == 0)
        {
            return;
        }

        // Dispose されたエフェクトを削除
        var count = _effects.RemoveWhere(effect => effect.IsDisposed);
        if (count > 0)
        {
            _isDirty = true;
        }

        if (!_isDirty)
        {
            return;
        }

        _isDirty = false;

        var reduceCoolDownRate = 0f;
        var manaRegenerationValue = 0;
        var manaRegenerationInterval = 0;

        foreach (var effect in _effects)
        {
            switch (effect)
            {
                case ReduceCoolDownRate reduceCoolDownRateEffect:
                {
                    reduceCoolDownRate += reduceCoolDownRateEffect.Value;
                    break;
                }
                case AddManaRegeneration addManaRegeneration:
                    manaRegenerationValue += addManaRegeneration.Value;
                    manaRegenerationInterval += addManaRegeneration.Interval;
                    break;
            }
        }

        OnSolveEffect(_effects);

        _coolDownReduceRateRp.Value = Math.Max(reduceCoolDownRate, 0);
        _manaRegenerationInterval = Math.Max(manaRegenerationInterval, 0);
        _manaRegenerationValue = Math.Max(manaRegenerationValue, 0);
    }
}