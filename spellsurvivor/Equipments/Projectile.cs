using System;
using Godot;
using R3;

namespace fms;

public partial class Projectile : Area2D
{
    private IDisposable? _disposable;
    private float _lifeTimeCounter;
    private Vector2 _velocity;

    [Export]
    public Vector2 Direction { get; set; } = Vector2.Right;

    [Export]
    public float InitialSpeed { get; set; } = 50f;

    [Export(PropertyHint.Range, "0, 100")]
    public float Acceleration { get; set; } = 50f;

    [Export(PropertyHint.Range, "0, 10")]
    public float LifeTime { get; set; } = 1f;

    public IEntity Instigator { get; internal set; } = null!;

    protected override void Dispose(bool disposing)
    {
        _disposable?.Dispose();
        base.Dispose(disposing);
    }

    public override void _Ready()
    {
        var d1 = this.BodyEnteredAsObservable()
            .Cast<Node2D, IEntity>()
            .Subscribe(this, (entity, state) => { state.ApplyDamageToEntity(entity); });

        _disposable = d1;

        _velocity = InitialSpeed * Direction;
    }

    public override void _Process(double delta)
    {
        // Move Projectile
        _velocity += (float)delta * Acceleration * Direction;
        Position += _velocity * (float)delta;

        // Update LifeTime
        _lifeTimeCounter += (float)delta;

        if (_lifeTimeCounter >= LifeTime)
        {
            KillThis();
        }
    }

    private void ApplyDamageToEntity(IEntity entity)
    {
        entity.TakeDamage(50, Instigator);
        KillThis();
    }

    private void KillThis()
    {
        QueueFree();
    }
}