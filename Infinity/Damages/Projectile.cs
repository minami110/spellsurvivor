using Godot;
using R3;

namespace fms;

public partial class Projectile : Area2D
{
    [Export]
    public float Damage { get; set; } = 10f;

    [Export]
    public Vector2 Direction { get; set; } = Vector2.Right;

    [Export]
    public float InitialSpeed { get; set; } = 50f;

    [Export(PropertyHint.Range, "0, 100")]
    public float Acceleration { get; set; } = 50f;

    [Export(PropertyHint.Range, "0, 10")]
    public float LifeTime { get; set; } = 1f;

    private float _lifeTimeCounter;
    private Vector2 _velocity;

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

        // Update LifeTime
        _lifeTimeCounter += (float)delta;

        if (_lifeTimeCounter >= LifeTime)
        {
            KillThis();
        }
    }

    private void ApplyDamageToEnemy(Enemy enemy)
    {
        enemy.TakeDamage(Damage);
        KillThis();
    }

    private void KillThis()
    {
        QueueFree();
    }
}