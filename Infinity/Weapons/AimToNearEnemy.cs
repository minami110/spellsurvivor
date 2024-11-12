using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class AimToNearEnemy : Area2D
{
    /// <summary>
    /// 対象とするターゲットのタイプ
    /// </summary>
    [Export]
    private AimTarget Target { get; set; } = AimTarget.Nearest;

    /// <summary>
    /// 敵を検索する範囲 (px)
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    public float SearchRadius
    {
        get => _searchRadius;
        set
        {
            if (Math.Abs(_searchRadius - value) <= 0.0001f)
            {
                return;
            }

            if (IsNodeReady())
            {
                UpdateCollisionRadius(value);
            }

            _searchRadius = value;
        }
    }

    /// <summary>
    /// 子の回転を更新するかどうか
    /// </summary>
    [Export]
    private bool UpdateRotation { get; set; } = true;

    /// <summary>
    /// 子の回転が有効な場合の回転感度
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    private float RotateSensitivity { get; set; } = 0.7f;

    public enum AimTarget
    {
        Nearest,
        Farthest
    }

    /// <summary>
    /// 範囲内に存在する敵のリスト
    /// </summary>
    public readonly List<EnemyBase> Enemies = new();

    private float _restAngle;

    private float _searchRadius = 100f;

    private float _targetAngle;

    /// <summary>
    /// 現在狙っている(有効な敵が存在する)かどうか
    /// </summary>
    public bool IsAiming { get; private set; }

    /// <summary>
    /// 範囲内の最も近い敵, 存在しない場合は null
    /// </summary>
    public EnemyBase? NearestEnemy { get; private set; }

    /// <summary>
    /// 範囲内の最も遠い敵, 存在しない場合は null
    /// </summary>
    public EnemyBase? FarthestEnemy { get; private set; }

    public override void _EnterTree()
    {
        // Subscribe to parent player's face direction
        var player = GetNode<BasePlayerPawn>("../.."); // ToDo: Hardcoded
        player.FaceDirection
            .Subscribe(x => { _restAngle = x == PawnFaceDirection.Right ? Mathf.Atan2(0, 1) : Mathf.Atan2(0, -1); })
            .AddTo(this);
        UpdateCollisionRadius(_searchRadius);
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateNearAndFarEnemy();

        if (Target == AimTarget.Nearest)
        {
            if (NearestEnemy is not null)
            {
                IsAiming = true;

                if (!UpdateRotation)
                {
                    return;
                }

                var targetPosition = NearestEnemy.GlobalPosition;
                var targetAngle = Mathf.Atan2(targetPosition.Y - GlobalPosition.Y, targetPosition.X - GlobalPosition.X);
                Rotation = Mathf.LerpAngle(Rotation, targetAngle, RotateSensitivity);
            }
            else
            {
                IsAiming = false;

                if (!UpdateRotation)
                {
                    return;
                }

                // Update Rotation
                Rotation = Mathf.LerpAngle(Rotation, _restAngle, RotateSensitivity);
            }
        }
        else if (Target == AimTarget.Farthest)
        {
            if (FarthestEnemy is not null)
            {
                IsAiming = true;

                if (!UpdateRotation)
                {
                    return;
                }

                var targetPosition = FarthestEnemy.GlobalPosition;
                var targetAngle = Mathf.Atan2(targetPosition.Y - GlobalPosition.Y, targetPosition.X - GlobalPosition.X);
                Rotation = Mathf.LerpAngle(Rotation, targetAngle, RotateSensitivity);
            }
            else
            {
                IsAiming = false;

                if (!UpdateRotation)
                {
                    return;
                }

                // Update Rotation
                Rotation = Mathf.LerpAngle(Rotation, _restAngle, RotateSensitivity);
            }
        }
    }

    private void UpdateCollisionRadius(float radius)
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
            collisionShape.DebugColor = new Color("c16e6500");
            AddChild(collisionShape);
        }

        // Update CircleShape2D radius, if not exist, create new one
        if (collisionShape.Shape is null)
        {
            var newCircleShape = new CircleShape2D();
            collisionShape.Shape = newCircleShape;
        }

        if (collisionShape.Shape is CircleShape2D circleShape)
        {
            circleShape.Radius = radius;
        }
        else
        {
            throw new InvalidOperationException("Shape must be CircleShape2D");
        }
    }

    private void UpdateNearAndFarEnemy()
    {
        NearestEnemy = null;
        FarthestEnemy = null;
        Enemies.Clear();

        var bodies = GetOverlappingBodies();
        var centerPosition = GlobalPosition;

        var minLen = float.MaxValue;
        var maxLen = float.MinValue;

        foreach (var o in bodies)
        {
            if (o is not EnemyBase e)
            {
                continue;
            }

            if (e.IsDead)
            {
                continue;
            }

            Enemies.Add(e);

            var distance = centerPosition.DistanceSquaredTo(e.GlobalPosition);
            if (distance < minLen)
            {
                minLen = distance;
                NearestEnemy = e;
            }

            if (distance > maxLen)
            {
                maxLen = distance;
                FarthestEnemy = e;
            }
        }
    }
}