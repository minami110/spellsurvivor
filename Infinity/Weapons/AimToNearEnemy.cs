using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

public partial class AimToNearEnemy : Area2D
{
    /// <summary>
    /// 対象とするターゲットのタイプ
    /// </summary>
    [Export]
    private AimTarget Target { get; set; } = AimTarget.Nearest;

    /// <summary>
    /// 自身を回転するかどうか
    /// </summary>
    [Export]
    private bool UpdateRotation { get; set; } = true;

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