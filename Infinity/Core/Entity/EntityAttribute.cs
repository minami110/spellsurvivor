using System;
using System.Runtime.CompilerServices;
using R3;

namespace fms;

public abstract class ReadOnlyEntityAttribute<T> : IDisposable
{
    public abstract Observable<T> ChangedCurrentValue { get; }

    /// <summary>
    /// Returns the default value of this attribute
    /// </summary>
    public abstract T DefaultValue { get; }

    /// <summary>
    /// Current value of this attribute
    /// </summary>
    public abstract T CurrentValue { get; }

    public abstract void Dispose();
}

public sealed class EntityAttribute<T> : ReadOnlyEntityAttribute<T> where T : IEquatable<T>
{
    private readonly BehaviorSubject<T> _changedCurrentValue;

    private T _currentValue;

    public override T DefaultValue { get; }

    public override Observable<T> ChangedCurrentValue => _changedCurrentValue;

    public override T CurrentValue => _currentValue;

    public EntityAttribute(T value)
    {
        DefaultValue = value;
        _changedCurrentValue = new BehaviorSubject<T>(value);
        _currentValue = value;
    }

    public override void Dispose()
    {
        _changedCurrentValue.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetToDefaultValue()
    {
        _currentValue = DefaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCurrentValue(T value)
    {
        if (value.Equals(_currentValue))
        {
            return;
        }

        _currentValue = value;
        _changedCurrentValue.OnNext(value);
    }
}