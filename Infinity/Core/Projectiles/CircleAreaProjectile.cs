﻿using System;
using Godot;

namespace fms.Projectile;

/// <summary>
/// Circle 形状の AreaProjectile
/// </summary>
public partial class CircleAreaProjectile : AreaProjectile
{
    private Vector2 _offset;
    private float _radius;

    public float Radius
    {
        get => _radius;
        set
        {
            if (Math.Abs(_radius - value) <= 0.0001f)
            {
                return;
            }

            if (IsNodeReady())
            {
                UpdateRadiusAndOffset(value, _offset);
            }

            _radius = value;
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
                UpdateRadiusAndOffset(_radius, value);
            }

            _offset = value;
        }
    }

    private protected virtual Color DebugColor { get; set; } = new("00a26e00");

    public override void _EnterTree()
    {
        Monitorable = false;
        CollisionLayer = Constant.LAYER_NONE;
        CollisionMask = Constant.LAYER_ENEMY;
        UpdateRadiusAndOffset(_radius, _offset);
    }

    private void UpdateRadiusAndOffset(float radius, in Vector2 offset)
    {
        var collisionShape = CollisionShape;

        // Note: ついでにデバッグ用の色も設定する
        collisionShape.DebugColor = DebugColor;

        // Update CircleShape2D radius, if not exist, create new one
        if (collisionShape.Shape is null)
        {
            var newShape = new CircleShape2D();
            collisionShape.Shape = newShape;
        }

        // 指定されたサイズに更新する
        if (collisionShape.Shape is CircleShape2D circleShape)
        {
            circleShape.Radius = radius;
            collisionShape.Position = offset;
        }
        else
        {
            throw new InvalidOperationException($"Shape must be {nameof(CircleShape2D)}");
        }
    }
}