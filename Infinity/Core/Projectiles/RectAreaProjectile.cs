using System;
using Godot;

namespace fms.Projectile;

/// <summary>
/// Rect 形状の AreaProjectile
/// </summary>
public partial class RectAreaProjectile : AreaProjectile
{
    private Vector2 _offset;
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
                UpdateSizeAndOffset(value, _offset);
            }

            _size = value;
        }
    }

    public Vector2 Offset
    {
        get => _offset;
        set
        {
            if (Math.Abs(_offset.X - value.X) <= 0.0001f && Math.Abs(_offset.Y - value.Y) <= 0.0001f)
            {
                return;
            }

            if (IsNodeReady())
            {
                UpdateSizeAndOffset(_size, value);
            }

            _offset = value;
        }
    }

    public override void _EnterTree()
    {
        Monitorable = false;
        CollisionLayer = Constant.LAYER_NONE;
        CollisionMask = Constant.LAYER_MOB;
        UpdateSizeAndOffset(_size, _offset);
    }

    private void UpdateSizeAndOffset(in Vector2 size, in Vector2 offset)
    {
        var collisionShape = CollisionShape;

        // Note: ついでにデバッグ用の色も設定する
        collisionShape.DebugColor = new Color("00a26e00");
        ;

        // Update CircleShape2D radius, if not exist, create new one
        if (collisionShape.Shape is null)
        {
            var newShape = new RectangleShape2D();
            collisionShape.Shape = newShape;
        }

        // 指定されたサイズに更新する
        if (collisionShape.Shape is RectangleShape2D rectShape)
        {
            rectShape.Size = size;
            collisionShape.Position = offset;
        }
        else
        {
            throw new InvalidOperationException($"Shape must be {nameof(RectangleShape2D)}");
        }
    }
}