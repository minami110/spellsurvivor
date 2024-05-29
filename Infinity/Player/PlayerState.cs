using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

/// <summary>
///     Player の体力やバフなどを管理するクラス
/// </summary>
public sealed class PlayerState : IEffectSolver, IDisposable
{
    private readonly IDisposable _disposable;

    private readonly List<EffectBase> _effects = new();

    private readonly ReactiveProperty<float> _health = new();
    private readonly ReactiveProperty<float> _maxHealth = new();
    private readonly ReactiveProperty<int> _money = new();
    private readonly ReactiveProperty<float> _moveSpeed = new();

    /// <summary>
    ///     現在の所持金
    /// </summary>
    public ReadOnlyReactiveProperty<int> Money => _money;

    /// <summary>
    ///     現在の移動速度
    /// </summary>
    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;

    /// <summary>
    ///     現在の体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> Health => _health;

    /// <summary>
    ///     最大体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;

    public PlayerState()
    {
        _disposable = Disposable.Combine(_health, _maxHealth);
    }

    public void Reset()
    {
        _health.Value = 0f;
        _maxHealth.Value = 0f;
        _money.Value = 0;
        _moveSpeed.Value = 0f;
        _effects.Clear();
    }

    public void Dispose()
    {
        _disposable.Dispose();
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
            switch (effect)
            {
                case AddMoneyEffect addMoneyEffect:
                {
                    _money.Value += (int)addMoneyEffect.Value;
                    break;
                }
                case AddMoveSpeedEffect addMoveSpeedEffect:
                {
                    _moveSpeed.Value += addMoveSpeedEffect.Value;
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