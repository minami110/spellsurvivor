using System;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

public partial class WeaponState : Node, IAttributeDictionary
{
    private readonly DamageAttribute _attackSpeed;
    private readonly Dictionary<string, Variant> _attributes = new();
    private readonly DamageAttribute _damage;
    private readonly EntityAttribute<uint> _knockback;

    private readonly EntityAttribute<uint> _level;

    public ReadOnlyEntityAttribute<uint> Level => _level;
    public ReadOnlyDamageAttribute Damage => _damage;
    public ReadOnlyDamageAttribute AttackSpeed => _attackSpeed;
    public ReadOnlyEntityAttribute<uint> Knockback => _knockback;

    // Parameterless constructor for Godot
    private WeaponState()
    {
        throw new InvalidOperationException("Do not use this constructor");
    }

    public WeaponState(uint level, uint damage, uint cooldown, float cooldownRate, uint knockback)
    {
        _level = new EntityAttribute<uint>(level);
        _damage = new DamageAttribute(damage, 1.0f);
        _attackSpeed = new DamageAttribute(cooldown, cooldownRate);
        _knockback = new EntityAttribute<uint>(knockback);
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