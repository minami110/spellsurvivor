using System.Runtime.CompilerServices;
using Godot;
using R3;

namespace fms;

public abstract class ReadOnlyDamageAttribute : ReadOnlyEntityAttribute<uint>
{
    public abstract Observable<float> ChangedRate { get; }

    /// <summary>
    /// Returns the default max value
    /// </summary>
    public abstract float DefaultRate { get; }

    /// <summary>
    /// Reutnrs the current max value
    /// </summary>
    public abstract float Rate { get; }
}

public sealed class DamageAttribute : ReadOnlyDamageAttribute
{
    private readonly BehaviorSubject<uint> _changedCurrentValue;
    private readonly BehaviorSubject<float> _changedRate;

    private uint _currentValue;
    private float _rate;

    /// <summary>
    /// </summary>
    public override float DefaultRate { get; }

    public override uint DefaultValue { get; }

    /// <summary>
    /// </summary>
    public override Observable<uint> ChangedCurrentValue => _changedCurrentValue;

    /// <summary>
    /// </summary>
    public override Observable<float> ChangedRate => _changedRate;

    public override uint CurrentValue => _currentValue;

    public override float Rate => _rate;

    public DamageAttribute(uint value, float rate)
    {
        DefaultValue = value;
        DefaultRate = rate;

        // ReactiveProperty のように Subscribe 時に即 OnNext が呼ばれたほうが色々かんたんなので
        // BehaviourSubject を使用
        _changedCurrentValue = new BehaviorSubject<uint>(value);
        _changedRate = new BehaviorSubject<float>(rate);

        _currentValue = value;
        _rate = DefaultRate;
    }

    public override void Dispose()
    {
        _changedCurrentValue.Dispose();
        _changedRate.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRate(float rate)
    {
        if (rate.Equals(_rate))
        {
            return;
        }

        _rate = rate;
        _changedRate.OnNext(rate);

        // Calculate new damage
        _currentValue = (uint)Mathf.Ceil(DefaultValue * rate);
    }
}