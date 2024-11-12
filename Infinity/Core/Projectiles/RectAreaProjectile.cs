using System;
using Godot;

namespace fms.Projectile;

/// <summary>
/// Rect 形状の AreaProjectile
/// </summary>
public partial class RectAreaProjectile : AreaProjectile
{
    private Vector2 _size;

    public Vector2 Size
    {
        get => _size;
        set
        {
            if (Math.Abs(_size.X - value.X) <= 0.0001f && Math.Abs(_size.Y - value.Y) <= 0.0001f)
            {
                return;
            }

            if (IsNodeReady())
            {
                UpdateSize(value);
            }

            _size = value;
        }
    }

    public override void _EnterTree()
    {
        Monitorable = false;
        CollisionLayer = Constant.LAYER_NONE;
        CollisionMask = Constant.LAYER_MOB;
        UpdateSize(_size);
    }

    private void UpdateSize(in Vector2 newSize)
    {
        // Find CollisionShape2D
        CollisionShape2D? collisionShape = null;
        foreach (var c in GetChildren())
        {
            if (c is not CollisionShape2D cs)
            {
                continue;
            }

            collisionShape = cs;
            break;
        }

        // Do not exist, create new one
        if (collisionShape is null)
        {
            collisionShape = new CollisionShape2D();
            collisionShape.DebugColor = new Color("00a26e00");
            AddChild(collisionShape);
        }

        // Update CircleShape2D radius, if not exist, create new one
        if (collisionShape.Shape is null)
        {
            var newShape = new RectangleShape2D();
            collisionShape.Shape = newShape;
        }

        // 指定されたサイズに更新する, オフセットを入れて Rect の左辺が原点に重なるようにする
        if (collisionShape.Shape is RectangleShape2D rectShape)
        {
            rectShape.Size = newSize;
            collisionShape.Position = new Vector2(newSize.X / 2f, 0f);
        }
        else
        {
            throw new InvalidOperationException("Shape must be RectangleShape2D");
        }
    }
}