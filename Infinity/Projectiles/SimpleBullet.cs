using Godot;
using R3;

namespace fms.Projectile;

public partial class SimpleBullet : ProjectileRigidBodyBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    public override void _Ready()
    {
        // Set rigidbody parameter
        LinearVelocity = InitialVelocity * InitialSpeed;
        Rotation = LinearVelocity.Angle();

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