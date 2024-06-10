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

    /// <summary>
    ///     敵にダメージを与えるクールダウン
    /// </summary>
    public int CoolDownFrame { get; set; }

    /// <summary>
    ///     的にダメージを与える範囲の半径 (px)
    /// </summary>
    public float DamageAreaRadius { get; set; }

    public override void _Ready()
    {
        // Set collision shape
        _enemyDamageCollisionShape.GlobalScale = new Vector2(DamageAreaRadius, DamageAreaRadius);

        // Move 
        _enemyDamageArea.GlobalPosition = InitialPosition;
    }

    public override void _Process(double delta)
    {
        if (CurrentFrame % CoolDownFrame != 0)
        {
            return;
        }

        var bodies = _enemyDamageArea.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is Enemy enemy)
            {
                enemy.TakeDamage(BaseDamage);
            }
        }
    }
}