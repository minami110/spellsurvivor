using Godot;

namespace fms.Projectile;

public partial class SizeMod : Node
{
    public int Add { get; init; } = 0;
    public float Multiply { get; init; } = 1.0f;

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        var col = projectile.GetNodeOrNull<CollisionShape2D>("DamageCollision");
        if (col != null)
        {
            var shape = (CircleShape2D)col.Shape;
            shape.Radius = (shape.Radius + Add) * Multiply;
        }
    }
}