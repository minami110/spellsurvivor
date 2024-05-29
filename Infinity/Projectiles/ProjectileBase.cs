using Godot;
using R3;

namespace fms;

public partial class ProjectileBase : Area2D
{
    private float _lifeTimeCounter;
    private Vector2 _velocity;
    public float Damage { get; set; } = 10f;

    public Vector2 Direction { get; set; } = Vector2.Right;

    public float InitialSpeed { get; set; } = 50f;

    public float Acceleration { get; set; } = 50f;

    public float LifeTime { get; set; } = 1f;

    public override void _Ready()
    {
        var d1 = this.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (entity, state) => { state.ApplyDamageToEnemy(entity); });

        Disposable.Combine(d1).AddTo(this);
        _velocity = InitialSpeed * Direction;
    }

    public override void _Process(double delta)
    {
        // Move Projectile
        _velocity += (float)delta * Acceleration * Direction;
        Position += _velocity * (float)delta;

        // Update Rotation
        Rotation = _velocity.Angle();

        // Update LifeTime
        _lifeTimeCounter += (float)delta;

        if (_lifeTimeCounter >= LifeTime)
        {
            KillThis();
        }
    }

    private protected virtual void ApplyDamageToEnemy(Enemy enemy)
    {
        enemy.TakeDamage(Damage);
        KillThis();
    }

    private void KillThis()
    {
        QueueFree();
    }
}