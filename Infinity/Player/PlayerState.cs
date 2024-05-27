using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

/// <summary>
///     Player の体力やバフなどを管理するクラス
/// </summary>
public partial class PlayerState : Node
{
    private readonly IDisposable _disposable;

    private readonly List<EffectBase> _effects = new();
    private readonly ReactiveProperty<float> _health = new();
    private readonly ReactiveProperty<float> _maxHealth = new();

    public PlayerState()
    {
        _disposable = Disposable.Combine(_health, _maxHealth);
    }

    /// <summary>
    ///     最大体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> Health => _health;

    /// <summary>
    /// 現在の体力
    /// </summary>
    public ReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;

    public void Reset()
    {
        _health.Value = 0f;
        _maxHealth.Value = 0f;
        _effects.Clear();
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

        var maxHealth = _health.Value;
        var health = _maxHealth.Value;
        var damage = 0f;

        foreach (var effect in _effects)
            switch (effect)
            {
                case AddHealthEffect addHealthEffect:
                {
                    maxHealth += addHealthEffect.Value;
                    break;
                }
                case AddMaxHealthEffect addMaxHealthEffect:
                {
                    health += addMaxHealthEffect.Value;
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

    protected override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }
}