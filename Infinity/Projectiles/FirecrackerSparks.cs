using Godot;

namespace fms.Projectile;

/// <summary>
///     範囲内にいる敵に定期的にダメージを与える
/// </summary>
public partial class FirecrackerSparks : ProjectileNode2DBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    [Export]
    private CollisionShape2D _enemyDamageCollisionShape = null!;

    public FirecrackerSparkData FirecrackerSparkDataSettings;

    /// <summary>
    ///     敵にダメージを与えるクールダウン
    /// </summary>
    private int CoolDownFrame { get; set; }

    /// <summary>
    ///     的にダメージを与える範囲の半径 (px)
    /// </summary>
    private float DamageAreaRadius { get; set; }

    public override void _Ready()
    {
        // init settings
        CoolDownFrame = FirecrackerSparkDataSettings.DamageCoolDownFrame;
        DamageAreaRadius = FirecrackerSparkDataSettings.DamageAreaRadius;

        // Set collision shape
        _enemyDamageCollisionShape.GlobalScale = new Vector2(DamageAreaRadius, DamageAreaRadius);
    }

    public override void _Process(double delta)
    {
        if (AgeFrame % CoolDownFrame != 0)
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