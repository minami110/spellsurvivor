using Godot;
using R3;

namespace fms.Projectile;

public partial class RotateDisk : ProjectileBase
{
    [Export]
    private RigidBody2D _rigidBody = null!;

    [Export]
    private Area2D _enemyDamageArea = null!;

    public float InitialSpeed { get; set; }
    public Vector2 InitialVelocity { get; set; }


    public override void _Ready()
    {
        // Set rigidbody parameter
        _rigidBody.GlobalPosition = InitialPosition;
        _rigidBody.LinearVelocity = InitialVelocity * InitialSpeed;
        _rigidBody.Rotation = _rigidBody.LinearVelocity.Angle();

        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
    }

    private void OnEnemyBodyEntered(Enemy enemy)
    {
        enemy.TakeDamage(BaseDamage);
        KillThis();
    }
}