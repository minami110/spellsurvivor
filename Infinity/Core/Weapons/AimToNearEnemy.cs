using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

/// <summary>
/// 近くの敵を狙う関連の計算を行うノード
/// Note: Collision がセットアップされていなくても自動で生成されます
/// </summary>
[GlobalClass]
[Obsolete("Use AimEntity instead")]
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
        get;
        set
        {
            if (Math.Abs(field - value) <= 0.0001f)
            {
                return;
            }

            if (IsNodeReady())
            {
                UpdateCollisionRadius(value);
            }

            field = value;
        }
    } = 100f;

    /// <summary>
    /// 子の回転を更新するかどうか
    /// </summary>
    [Export]
    private bool UpdateRotation { get; set; } = true;

    /// <summary>
    /// 子の回転が有効な場合の回転感度
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RotateSensitivity { get; set; } = 0.3f;

    public enum AimTarget
    {
        Nearest,
        Farthest
    }

    private readonly Subject<Unit> _enteredAnyEnemy = new();

    /// <summary>
    /// 範囲内に存在する敵のリスト
    /// </summary>
    public readonly List<EntityEnemy> Enemies = new();

    private Vector2? _prevPosition;

    private float _restAngle;

    private float _targetAngle;
    public Observable<Unit> EnteredAnyEnemy => _enteredAnyEnemy;

    /// <summary>
    /// 現在狙っている(有効な敵が存在する)かどうか
    /// </summary>
    public bool IsAiming { get; private set; }

    /// <summary>
    /// 範囲内の最も近い敵, 存在しない場合は null
    /// </summary>
    public EntityEnemy? NearestEnemy { get; private set; }

    /// <summary>
    /// 範囲内の最も遠い敵, 存在しない場合は null
    /// </summary>
    public EntityEnemy? FarthestEnemy { get; private set; }

    public override void _EnterTree()
    {
        // 継承元のパラメーターの初期化
        // Note: 他の Area などに監視される必要がない設定
        Monitorable = false;
        CollisionLayer = Constant.LAYER_NONE;
        // Note: 現在プレイヤーしか使っていないので敵のみを検知する設定
        CollisionMask = Constant.LAYER_ENEMY;

        // 子の Shape を初期化する
        UpdateCollisionRadius(SearchRadius);

        // Disposable 関連
        _enteredAnyEnemy.AddTo(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_prevPosition.HasValue)
        {
            var vel = GlobalPosition - _prevPosition.Value;
            if (vel.LengthSquared() > 0)
            {
                var isRight = vel.Dot(Vector2.Right) > 0;
                _restAngle = isRight ? 0f : Mathf.Pi;
            }
        }

        UpdateNearAndFarEnemy();
        AimAtTarget();
        UpdateSpriteFlip();

        _prevPosition = GlobalPosition;
    }

    private void AimAtTarget()
    {
        var targetEnemy = Target == AimTarget.Nearest ? NearestEnemy : FarthestEnemy;

        if (targetEnemy is not null)
        {
            IsAiming = true;
            if (!UpdateRotation)
            {
                return;
            }

            RotateTowards(targetEnemy.GlobalPosition);
        }
        else
        {
            IsAiming = false;
            if (!UpdateRotation)
            {
                return;
            }

            RotateTowardsRestAngle();
        }
    }

    private void RotateTowards(Vector2 targetPosition)
    {
        var targetAngle = Mathf.Atan2(targetPosition.Y - GlobalPosition.Y, targetPosition.X - GlobalPosition.X);
        Rotation = Mathf.LerpAngle(Rotation, targetAngle, RotateSensitivity);
    }

    private void RotateTowardsRestAngle()
    {
        Rotation = Mathf.LerpAngle(Rotation, _restAngle, RotateSensitivity);
    }

    private void UpdateCollisionRadius(float radius)
    {
        // Find CollisionShape2D
        var collisionShape = this.FindFirstChild<CollisionShape2D>();

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
            if (o is not EntityEnemy e)
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

        if (NearestEnemy is not null || FarthestEnemy is not null)
        {
            _enteredAnyEnemy.OnNext(Unit.Default);
        }
    }

    private void UpdateSpriteFlip()
    {
        var sprite = GetNodeOrNull<Sprite2D>("%Sprite");
        if (sprite is null)
        {
            return;
        }

        // Rotation のベクトルが右向きか左向きかを判定する
        var rot = Rotation;
        var vec = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot));
        var isRight = vec.Dot(Vector2.Right) > 0;

        // Rotation が右半分にいるときと左半分にいるときで Sprite の Flip を変更する
        sprite.FlipV = !isRight;
    }
}