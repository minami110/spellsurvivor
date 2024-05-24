using System;
using Godot;
using R3;

namespace fms;

public partial class PlayerState : Node
{
    private readonly IDisposable _disposable;

    /// <summary>
    /// </summary>
    public readonly ReactiveProperty<float> Health;

    public readonly ReactiveProperty<float> MaxHealth;

    public PlayerState()
    {
        Health = new ReactiveProperty<float>(100f);
        MaxHealth = new ReactiveProperty<float>(100f);
        _disposable = Disposable.Combine(Health, MaxHealth);
    }

    protected override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }
}