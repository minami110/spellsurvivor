using System;
using R3;

namespace fms;

public sealed class EnemyState : IEffectSolver, IDisposable
{
    private readonly ReactiveProperty<float> _maxHealth = new();
    private readonly ReactiveProperty<float> _moveSpeed = new();

    /// <summary>
    /// 現在の移動速度
    /// </summary>
    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;

    public EntityHealth Health { get; }

    public EnemyState(uint baseMoveSpeed, uint baseMaxHealth)
    {
        _moveSpeed.Value = baseMoveSpeed;
        Health = new EntityHealth(baseMaxHealth, baseMaxHealth);
    }

    public void ApplyDamage(uint amount)
    {
        if (Health.CurrentValue < amount)
        {
            Health.SetCurrentValue(0u);
        }
        else
        {
            Health.SetCurrentValue(Health.CurrentValue - amount);
        }
    }

    public void Dispose()
    {
        Health.Dispose();
        _maxHealth.Dispose();
        _moveSpeed.Dispose();
    }

    public void AddEffect(EffectBase effect)
    {
    }

    public void SolveEffect()
    {
    }
}