using System;
using System.Runtime.CompilerServices;
using R3;

namespace fms;

public abstract class ReadOnlyEntityHealth : ReadOnlyEntityAttribute<uint>
{
    public abstract Observable<uint> ChangedMaxValue { get; }

    /// <summary>
    /// Returns the default max value
    /// </summary>
    public abstract uint DefaultMaxValue { get; }

    /// <summary>
    /// Reutnrs the current max value
    /// </summary>
    public abstract uint MaxValue { get; }
}

public sealed class EntityHealth : ReadOnlyEntityHealth, IDisposable
{
    private readonly BehaviorSubject<uint> _changedCurrentValue;
    private readonly BehaviorSubject<uint> _changedMaxValue;

    private uint _currentValue;
    private uint _maxValue;

    /// <summary>
    /// </summary>
    public override uint DefaultMaxValue { get; }

    public override uint DefaultValue { get; }

    /// <summary>
    /// </summary>
    public override Observable<uint> ChangedCurrentValue => _changedCurrentValue;

    /// <summary>
    /// </summary>
    public override Observable<uint> ChangedMaxValue => _changedMaxValue;

    public override uint CurrentValue => _currentValue;

    public override uint MaxValue => _maxValue;

    public EntityHealth(uint value, uint maxValue)
    {
        var v = Math.Min(value, maxValue);
        DefaultValue = v;
        DefaultMaxValue = maxValue;

        // ReactiveProperty のように Subscribe 時に即 OnNext が呼ばれたほうが色々かんたんなので
        // BehaviourSubject を使用
        _changedCurrentValue = new BehaviorSubject<uint>(v);
        _changedMaxValue = new BehaviorSubject<uint>(maxValue);

        _currentValue = v;
        _maxValue = DefaultMaxValue;
    }

    public void ResetToMaxValue()
    {
        SetCurrentValue(MaxValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCurrentValue(uint value)
    {
        var newValue = Math.Clamp(value, 0u, MaxValue);
        if (newValue != _currentValue)
        {
            _currentValue = newValue;
            _changedCurrentValue.OnNext(newValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMaxValue(uint value)
    {
        if (value != _maxValue)
        {
            if (value < _currentValue)
            {
                throw new NotImplementedException("MaxValue cannot be less than CurrentValue");
            }

            _maxValue = value;
            _changedMaxValue.OnNext(value);
        }
    }

    public override void Dispose()
    {
        _changedCurrentValue.Dispose();
        _changedMaxValue.Dispose();
    }
}