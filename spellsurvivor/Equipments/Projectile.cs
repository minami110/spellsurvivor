#nullable enable
using Godot;

namespace spellsurvivor;

public partial class Projectile : Area2D
{
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

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;

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

    private void OnAreaEntered(Area2D area)
    {
        if (area is not IEntity entity)
        {
            return;
        }

        entity.TakeDamage(50, Instigator);
        KillThis();
    }

    private void KillThis()
    {
        QueueFree();
    }
}