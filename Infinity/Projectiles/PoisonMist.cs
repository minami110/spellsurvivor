using Godot;

namespace fms.Projectile;

/// <summary>
///     範囲内にいる敵に定期的にダメージを与える
/// </summary>
public partial class PoisonMist : ProjectileBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    [Export]
    private CollisionShape2D _enemyDamageCollisionShape = null!;

    private int _coolDownCounter;

    public int CoolDown { get; set; }

    public float Radius { get; set; }

    public override void _Ready()
    {
        // Set collision shape
        _enemyDamageCollisionShape.GlobalScale = new Vector2(Radius, Radius);

        // Move 
        _enemyDamageArea.GlobalPosition = InitialPosition;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_coolDownCounter > CoolDown)
        {
            var bodies = _enemyDamageArea.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Enemy enemy)
                {
                    enemy.TakeDamage(BaseDamage);
                }
            }

            _coolDownCounter = 0;
            return;
        }

        _coolDownCounter++;
    }
}