using System;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

public partial class WeaponState : Node, IAttributeDictionary
{
    private readonly EntityRateAttribute _attackSpeed;
    private readonly Dictionary<string, Variant> _attributes = new();
    private readonly EntityRateAttribute _damage;
    private readonly EntityAttribute<uint> _knockback;
    private readonly EntityAttribute<uint> _level;
    private readonly EntityAttribute<float> _lifestealRate;

    public ReadOnlyEntityAttribute<uint> Level => _level;
    public ReadOnlyEntityRateAttribute Damage => _damage;
    public ReadOnlyEntityRateAttribute AttackSpeed => _attackSpeed;
    public ReadOnlyEntityAttribute<uint> Knockback => _knockback;
    public ReadOnlyEntityAttribute<float> LifestealRate => _lifestealRate;

    // Parameterless constructor for Godot
    private WeaponState()
    {
        throw new InvalidOperationException("Do not use this constructor");
    }

    public WeaponState(uint level, uint damage, uint cooldown, float cooldownRate, uint knockback,
        float lifestealRate = 0.0f)
    {
        _level = new EntityAttribute<uint>(level);
        _damage = new EntityRateAttribute(damage, 1.0f);
        _attackSpeed = new EntityRateAttribute(cooldown, cooldownRate);
        _knockback = new EntityAttribute<uint>(knockback);
        _lifestealRate = new EntityAttribute<float>(lifestealRate);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationExitTree)
        {
            // Reactive Properties の Dispose をまとめる
            Disposable.Combine(_level, _damage, _attackSpeed, _knockback).Dispose();
        }
    }

    public void SetLevel(uint value)
    {
        if (value == _level.CurrentValue)
        {
            return;
        }

        _level.SetCurrentValue(value);
    }

    private void OnUpdateAnyAttribute()
    {
        // Damage & Damage Rate
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.DamageRate, out var v))
            {
                var newValue = _damage.DefaultRate + (float)v;
                _damage.SetRate(newValue);
            }
        }
        // Coolldown SpeedRate
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.SpeedRate, out var v))
            {
                var newValue = _attackSpeed.DefaultRate + (float)v;
                _attackSpeed.SetRate(newValue);
            }
        }
        // Lifesteal Rate
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.LifestealRate, out var v))
            {
                var newRate = _lifestealRate.DefaultValue + (float)v;
                _lifestealRate.SetCurrentValue(newRate);
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