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

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            if (!IsInGroup(Constant.GroupNamePlayerState))
            {
                AddToGroup(Constant.GroupNamePlayerState);
            }
        }
        else if (what == NotificationProcess)
        {
            SolveEffect();
        }
        else if (what == NotificationExitTree)
        {
            _health.Dispose();
            _maxHealth.Dispose();
            _money.Dispose();
            _moveSpeed.Dispose();
        }
    }

    public void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
        _isDirty = true;
    }

    public void Reset()
    {
        _health.Value = 0f;
        _maxHealth.Value = 0f;
        _money.Value = 0;
        _moveSpeed.Value = 0f;
        _effects.Clear();
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
        }

        // ダメージの計算
        health -= damage;

        // 最大体力と体力の計算
        health = Mathf.Min(health, maxHealth);

        // 最終的な値を計算する
        _money.Value = (uint)Math.Max(0, money);
        _maxHealth.Value = maxHealth;
        _health.Value = health;
        _moveSpeed.Value = moveSpeed;
    }
}