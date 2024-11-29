using System;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

public partial class WeaponState : Node, IAttributeDictionary
{
    private readonly Dictionary<string, Variant> _attributes = new();
    private readonly DamageAttribute _cooldown;
    private readonly DamageAttribute _damage;
    private readonly EntityAttribute<uint> _knockback;

    private readonly EntityAttribute<uint> _level;

    public ReadOnlyEntityAttribute<uint> Level => _level;
    public ReadOnlyDamageAttribute Damage => _damage;
    public ReadOnlyDamageAttribute Cooldown => _cooldown;
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
        _cooldown = new DamageAttribute(cooldown, cooldownRate);
        _knockback = new EntityAttribute<uint>(knockback);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationExitTree)
        {
            // Reactive Properties の Dispose をまとめる
            Disposable.Combine(_level, _damage, _cooldown, _knockback).Dispose();
            ;
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
                var newValue = _damage.Rate + (float)v;
                _damage.SetRate(newValue);
            }
        }
        // Coolldown SpeedRate
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.SpeedRate, out var v))
            {
                var newValue = _cooldown.Rate + (float)v;
                _cooldown.SetRate(newValue);
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