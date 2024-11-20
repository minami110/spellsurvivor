using System;
using System.Collections.Generic;
using fms.Effect;
using Godot;
using R3;

namespace fms;

public sealed class EnemyState : IEffectSolver, IDisposable
{
    private readonly List<EffectBase> _effects = new();
    private readonly ReactiveProperty<float> _health = new();
    private readonly ReactiveProperty<float> _maxHealth = new();
    private readonly ReactiveProperty<float> _moveSpeed = new();

    /// <summary>
    /// 現在の移動速度
    /// </summary>
    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;

    /// <summary>
    /// 最大体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> Health => _health;

    /// <summary>
    /// 現在の体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;

    public void Dispose()
    {
        _health.Dispose();
        _maxHealth.Dispose();
        _moveSpeed.Dispose();
    }

    public void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
    }

    public void SolveEffect()
    {
        if (_effects.Count == 0)
        {
            return;
        }

        var maxHealth = _maxHealth.Value;
        var health = _health.Value;
        var damage = 0f;

        foreach (var effect in _effects)
        {
            switch (effect)
            {
                case Wing wing:
                {
                    _moveSpeed.Value += wing.Amount;
                    break;
                }
                case AddHealthEffect addHealthEffect:
                {
                    health += addHealthEffect.Value;
                    break;
                }
                case AddMaxHealthEffect addMaxHealthEffect:
                {
                    maxHealth += addMaxHealthEffect.Value;
                    break;
                }
                case PhysicalDamageEffect physicalDamageEffect:
                {
                    damage += physicalDamageEffect.Value;
                    break;
                }
            }
        }

        // Clear effects
        _effects.Clear();

        // 最大体力と体力の計算
        health = Mathf.Min(health, maxHealth);

        // ダメージの計算
        health -= damage;

        // 最終的な値を計算する
        _maxHealth.Value = maxHealth;
        _health.Value = health;
    }
}