using System;
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
            var shape = col.Shape;
            if (shape is CircleShape2D circle)
            {
                circle.Radius = (circle.Radius + Add) * Multiply;
            }
            else if (shape is RectangleShape2D rect)
            {
                throw new NotImplementedException("RectangleShape2D is not implemented yet.");
            }
        }
    }
}