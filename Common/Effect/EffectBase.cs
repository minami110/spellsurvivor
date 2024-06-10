using System;

namespace fms;

public abstract class EffectBase : IDisposable
{
    public bool IsDisposed { get; private set; }

    public virtual void OnSolved()
    {
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }
}