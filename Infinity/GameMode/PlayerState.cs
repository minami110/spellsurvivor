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

    private readonly HashSet<EffectBase> _effects = new();

    private readonly ReactiveProperty<float> _health = new();
    private readonly ReactiveProperty<float> _maxHealth = new();
    private readonly ReactiveProperty<uint> _money = new();
    private readonly ReactiveProperty<float> _moveSpeed = new();

    private bool _isDirty;

    /// <summary>
    ///     現在の所持金
    /// </summary>
    public ReadOnlyReactiveProperty<uint> Money => _money;

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
        _isDirty = true;
    }

    public void SolveEffect()
    {
        if (!_isDirty || _effects.Count == 0)
        {
            return;
        }

        _isDirty = false;

        // Dispose されたエフェクトを削除
        _effects.RemoveWhere(effect => effect.IsDisposed);

        // スタートとなるパラメーターを用意
        var maxHealth = 0f;
        var health = 0f;
        var damage = 0f;
        var money = 0;
        var moveSpeed = 0f;

        // IsSolved が false のエフェクトを解決する
        foreach (var effect in _effects)
        {
            switch (effect)
            {
                case MoneyEffect moneyEffect:
                {
                    money += moneyEffect.Value;
                    break;
                }
                case AddMoveSpeedEffect addMoveSpeedEffect:
                {
                    moveSpeed += addMoveSpeedEffect.Value;
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

            effect.OnSolved();
        }

        // 最大体力と体力の計算
        health = Mathf.Min(health, maxHealth);

        // ダメージの計算
        health -= damage;

        // 最終的な値を計算する
        _maxHealth.Value = maxHealth;
        _health.Value = health;
        _money.Value = (uint)Math.Max(0, money);
        _moveSpeed.Value = moveSpeed;
    }
}