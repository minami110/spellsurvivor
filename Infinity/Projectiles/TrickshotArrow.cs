using Godot;
using R3;

namespace fms.Projectile;

public partial class TrickshotArrow : ProjectileBase
{
    [Export]
    private RigidBody2D _rigidBody = null!;

    [Export]
    private Area2D _enemyDamageArea = null!;

    [Export]
    private Area2D _enemySearchArea = null!;

    [Export]
    private CollisionShape2D _enemySearchCollisionShape = null!;

    [Export]
    private TextureRect _texture = null!;

    private int _bounceCounter;

    private Enemy? _previousEnemy;

    public float BaseSpeed { get; set; }
    public Vector2 InitialVelocity { get; set; }

    public int BounceCount { get; set; }
    public float BounceDamageMultiplier { get; set; }
    public float BounceSearchRadius { get; set; }


    public override void _Ready()
    {
        // Set rigidbody parameter
        _rigidBody.GlobalPosition = InitialPosition;
        _rigidBody.LinearVelocity = InitialVelocity * BaseSpeed;
        _rigidBody.Rotation = _rigidBody.LinearVelocity.Angle();

        if (BounceCount > 0)
        {
            // Set collision shape
            _enemySearchCollisionShape.Disabled = false;
            _enemySearchCollisionShape.GlobalScale = new Vector2(BounceSearchRadius, BounceSearchRadius);
        }

        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
    }


    private void OnEnemyBodyEntered(Enemy enemy)
    {
        if (_previousEnemy == enemy)
        {
            return;
        }

        var damage = BaseDamage;
        if (_bounceCounter > 0)
        {
            damage *= BounceDamageMultiplier;
        }

        enemy.TakeDamage(damage);
        _previousEnemy = enemy;

        if (_bounceCounter >= BounceCount)
        {
            KillThis();
            return;
        }

        // Bounce!
        ResetLifeFrameCounter();
        _texture.Modulate = new Color(0, 1, 0);
        _bounceCounter++;

        if (TryGetNearestEnemy(_previousEnemy, out var nextEnemy))
        {
            // Update Direction
            _rigidBody.LinearVelocity =
                (nextEnemy!.GlobalPosition - _rigidBody.GlobalPosition).Normalized() * BaseSpeed;
            _rigidBody.Rotation = _rigidBody.LinearVelocity.Angle();
        }
        else
        {
            KillThis();
        }
    }

    private bool TryGetNearestEnemy(Enemy? ignore, out Enemy? nearestEnemy)
    {
        nearestEnemy = null;
        var distance = 99999f;

        var overlappingArea = _enemySearchArea.GetOverlappingBodies();
        if (overlappingArea.Count <= 0)
        {
            return false;
        }

        foreach (var body in overlappingArea)
        {
            if (body is Enemy enemy && enemy != ignore)
            {
                var d = _rigidBody.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (d < distance)
                {
                    distance = d;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy is not null;
    }
}