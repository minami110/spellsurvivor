using System;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

/// <summary>
/// Entity の体力やバフなどを管理するクラス
/// </summary>
public partial class EntityState : Node, IAttributeDictionary
{
    private readonly Dictionary<string, Variant> _attributes = new();
    private readonly EntityAttribute<float> _dodgeRate;
    private readonly EntityAttribute<uint> _goldNugget;
    private readonly EntityHealth _health;
    private readonly EntityAttribute<uint> _money;
    private readonly EntityAttribute<float> _moveSpeed;

    // ===== Begin Stats =====
    /// <summary>
    /// 現在の所持金
    /// </summary>
    public ReadOnlyEntityAttribute<uint> Money => _money;

    /// <summary>
    /// </summary>
    public ReadOnlyEntityAttribute<uint> GoldNugget => _goldNugget;

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

    private EntityState()
    {
        // Parameterless constructor for Godot
        throw new ApplicationException("Parameterless constructor is not supported");
    }

    public EntityState(uint money, uint maxHealth, uint moveSpeed, float dodgeRate)
    {
        _money = new EntityAttribute<uint>(money);
        _health = new EntityHealth(maxHealth, maxHealth);
        _moveSpeed = new EntityAttribute<float>(moveSpeed);
        _dodgeRate = new EntityAttribute<float>(dodgeRate);

        // 以下は引数に未対応
        _goldNugget = new EntityAttribute<uint>(0);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationExitTree)
        {
            // Reactive Properties の Dispose をまとめる
            var d = Disposable.Combine(_money, _goldNugget, _moveSpeed, _health, _dodgeRate);
            d.Dispose();
        }
    }

    public void AddGoldNugget(uint amount)
    {
        _goldNugget.SetCurrentValue(_goldNugget.CurrentValue + amount);
    }

    public void AddMoney(uint amount)
    {
        _money.SetCurrentValue(_money.CurrentValue + amount);
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

    public void ReduceMoney(uint amount)
    {
        if (_money.CurrentValue < amount)
        {
            throw new NotImplementedException("所持金が足りません");
        }

        _money.SetCurrentValue(_money.CurrentValue - amount);
    }

    public void ResetToMaxHealth()
    {
        _health.ResetToMaxValue();
    }

    private void OnUpdateAnyAttribute()
    {
        // MaxHealth
        {
            if (_attributes.TryGetValue(EntityAttributeNames.MaxHealth, out var v))
            {
                var maxHealth = _health.DefaultMaxValue + (uint)v;
                _health.SetMaxValue(maxHealth);
            }
        }
        // MoveSpeed
        {
            if (_attributes.TryGetValue(EntityAttributeNames.MoveSpeed, out var v))
            {
                var moveSpeed = _moveSpeed.DefaultValue + (float)v;
                _moveSpeed.SetCurrentValue(moveSpeed);
            }
        }
        // DodgeRate
        {
            if (_attributes.TryGetValue(EntityAttributeNames.DodgeRate, out var v))
            {
                var dodgeRate = Mathf.Clamp(_dodgeRate.DefaultValue + (float)v, 0f, 1f);
                _dodgeRate.SetCurrentValue(dodgeRate);
            }
        }
    }

    bool IAttributeDictionary.TryGetAttribute(string key, out Variant value)
    {
        return _attributes.TryGetValue(key, out value);
    }

    void IAttributeDictionary.SetAttribute(string key, Variant value)
    {
        _attributes[key] = value;
        OnUpdateAnyAttribute();
    }
}