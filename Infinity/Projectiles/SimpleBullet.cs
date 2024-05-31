using System;
using Godot;
using R3;

namespace fms;

public partial class SimpleBullet : Area2D
{
    public enum KillType
    {
        Hit,
        TimeOut
    }

    private readonly Subject<KillReason> _killedSubject = new();

    private IDisposable? _bodyEnteredDisposable;

    private bool _isDead;
    private float _lifeTimeCounter;
    private Vector2 _velocity;
    public float Damage { get; set; } = 10f;

    public Vector2 Direction { get; set; } = Vector2.Right;

    public float InitialSpeed { get; set; } = 50f;

    public float Acceleration { get; set; } = 50f;

    public float LifeTime { get; set; } = 1f;

    public Observable<KillReason> Killed => _killedSubject;

    public override void _Ready()
    {
        _bodyEnteredDisposable = this.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (entity, state) => { state.OnEnemyBodyEntered(entity); });

        _velocity = InitialSpeed * Direction;
    }

    public override void _Process(double delta)
    {
        if (_isDead)
        {
            return;
        }

        // Move Projectile
        _velocity += (float)delta * Acceleration * Direction;
        Position += _velocity * (float)delta;

        // Update Rotation
        Rotation = _velocity.Angle();

        // Update LifeTime
        _lifeTimeCounter += (float)delta;

        if (_lifeTimeCounter >= LifeTime)
        {
            KillThis(KillType.TimeOut);
        }
    }

    public override void _ExitTree()
    {
        _killedSubject.Dispose();
        _bodyEnteredDisposable?.Dispose();
        _bodyEnteredDisposable = null;
    }

    private protected virtual void ApplyDamageToEnemy(Enemy enemy)
    {
        if (_isDead)
        {
            return;
        }

        _bodyEnteredDisposable?.Dispose();
        _bodyEnteredDisposable = null;

        enemy.TakeDamage(Damage);
        KillThis(KillType.Hit);
    }

    private protected virtual void OnEnemyBodyEntered(Enemy enemy)
    {
        ApplyDamageToEnemy(enemy);
    }

    private void KillThis(KillType type)
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _killedSubject.OnNext(new KillReason { Why = type, Position = GlobalPosition });
        _killedSubject.OnCompleted();

        CallDeferred(Node.MethodName.QueueFree);
    }

    public readonly struct KillReason
    {
        public required KillType Why { get; init; }
        public required Vector2 Position { get; init; }
    }
}