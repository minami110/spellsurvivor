using System;
using System.Collections.Generic;
using fms.Effect;
using Godot;
using R3;

namespace fms;

/// <summary>
/// Entity の体力やバフなどを管理するクラス
/// </summary>
public partial class EntityState : Node
{
    private readonly EntityAttribute<float> _dodgeRate;
    private readonly HashSet<EffectBase> _effects = new();
    private readonly EntityHealth _health;
    private readonly ReactiveProperty<uint> _money = new();
    private readonly EntityAttribute<float> _moveSpeed;

    // ===== Begin Stats =====

    private bool _isDirty;

    /// <summary>
    /// 現在の所持金
    /// </summary>
    // ToDo: これは Entity State ではないので, Main / Shop などの Game 側で管理する
    [Obsolete]
    public ReadOnlyReactiveProperty<uint> Money => _money;

    /// <summary>
    /// 現在の移動速度
    /// </summary>
    public ReadOnlyEntityAttribute<float> MoveSpeed => _moveSpeed;

    /// <summary>
    /// 回避率 (0.0 ~ 1.0), 1 を超えた値は 1 に丸められる
    /// </summary>
    public ReadOnlyEntityAttribute<float> DodgeRate => _dodgeRate;

    /// <summary>
    /// 現在の体力, 最大体力
    /// </summary>
    public ReadOnlyEntityHealth Health => _health;

    // Parameterless constructor for Godot
    private EntityState()
    {
        _health = new EntityHealth(0u, 0u);
        _moveSpeed = new EntityAttribute<float>(0f);
        _dodgeRate = new EntityAttribute<float>(0f);
    }

    public EntityState(uint maxHealth, uint moveSpeed, float dodgeRate)
    {
        _health = new EntityHealth(maxHealth, maxHealth);
        _moveSpeed = new EntityAttribute<float>(moveSpeed);
        _dodgeRate = new EntityAttribute<float>(dodgeRate);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Note: Process を override していないのでここで手動で有効化する
            SetProcess(true);

            // Reactive Properties の Dispose をまとめる
            var d = Disposable.Combine(_money, _moveSpeed, _health, _dodgeRate);
            d.AddTo(this);
        }
        else if (what == NotificationProcess)
        {
            SolveEffect();
        }
    }

    public void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
        _isDirty = true;
    }

    [Obsolete]
    public void AddMoney(uint amount)
    {
        _money.Value += amount;
    }

    public void ApplyDamage(uint amount)
    {
        if (_health.CurrentValue < amount)
        {
            _health.SetCurrentValue(0u);
        }
        else
        {
            _health.SetCurrentValue(_health.CurrentValue - amount);
        }
    }

    public void Heal(uint amount)
    {
        _health.SetCurrentValue(_health.CurrentValue + amount);
    }

    [Obsolete]
    public void ReduceMoney(uint amount)
    {
        if (_money.Value < amount)
        {
            throw new NotImplementedException("所持金が足りません");
        }

        _money.Value -= amount;
    }

    public void ResetToMaxHealth()
    {
        _health.ResetToMaxValue();
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

        // スタートとなるパラメーターを用意
        var maxHealth = _health.DefaultMaxValue;
        var moveSpeed = _moveSpeed.CurrentValue;
        var dodgeRate = 0f;

        // IsSolved が false のエフェクトを解決する
        foreach (var effect in _effects)
        {
            switch (effect)
            {
                case Wing wing:
                {
                    moveSpeed += wing.Amount;
                    break;
                }
                case Heart heart:
                {
                    maxHealth += heart.Amount;
                    break;
                }
                case Dodge dodgeEffect:
                {
                    dodgeRate += dodgeEffect.Rate;
                    break;
                }
            }
        }

        // 最終的な値を計算する
        _health.SetMaxValue(maxHealth);
        _moveSpeed.SetCurrentValue(moveSpeed);
        _dodgeRate.SetCurrentValue(Mathf.Clamp(dodgeRate, 0f, 1f));
    }
}