using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms;

/// <summary>
/// 近くの敵を狙う関連の計算を行うノード
/// </summary>
[GlobalClass]
public partial class AimEntity : Area2D
{
    /// <summary>
    /// 対象とするターゲットのタイプ
    /// </summary>
    [Export]
    public TargetMode Mode { get; set; } = TargetMode.NearestEntity;

    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    public float MinRange
    {
        get;
        set
        {
            if (Math.Abs(field - value) <= 0.0001f)
            {
                return;
            }

            field = value;
        }
    }

    [Export(PropertyHint.Range, "0,200,,or_greater,suffix:px")]
    public float MaxRange
    {
        get;
        set
        {
            if (Math.Abs(field - value) <= 0.0001f)
            {
                return;
            }

            field = value;

            if (IsNodeReady())
            {
                UpdateCollisionRadius(value);
            }
        }
    } = 100f;

    /// <summary>
    /// 子の回転が有効な場合の回転感度
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RotateSensitivity { get; set; } = 0.3f;

    public enum TargetMode
    {
        NearestEntity,
        FarthestEntity
    }

    private readonly Subject<IEntity> _enteredAnyEntity = new();

    /// <summary>
    /// 範囲内に存在する敵のリスト
    /// </summary>
    public readonly List<IEntity> Entities = new();

    private IEntity? _farthestEntity;

    private IEntity? _nearestEntity;

    private Vector2? _prevPosition;
    private float _restAngle;
    private float _targetAngle;

    /// <summary>
    /// 現在狙っている対象との角度差 (Rad)
    /// </summary>
    public float AngleDiff => Mathf.AngleDifference(Rotation, _targetAngle);

    public Observable<IEntity> EnteredAnyEntity => _enteredAnyEntity;

    /// <summary>
    /// 現在狙っている(有効な敵が存在する)かどうか
    /// </summary>
    public bool IsAiming => TargetEntity is not null;

    /// <summary>
    /// </summary>
    public IEntity? TargetEntity
    {
        get
        {
            return Mode switch
            {
                TargetMode.NearestEntity => _nearestEntity,
                TargetMode.FarthestEntity => _farthestEntity,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public override void _EnterTree()
    {
        // 継承元のパラメーターの初期化
        // Note: 他の Area などに監視される必要がない設定
        Monitorable = false;
        CollisionLayer = Constant.LAYER_NONE;
        // Note: 現在プレイヤーしか使っていないので敵のみを検知する設定
        CollisionMask = Constant.LAYER_MOB;

        // 子の Shape を初期化する
        UpdateCollisionRadius(MaxRange);

        // Disposable 関連
        _enteredAnyEntity.AddTo(this);
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

        SearchTarget();

        // 通知をおくる
        if (TargetEntity is not null)
        {
            _enteredAnyEntity.OnNext(TargetEntity);
        }

        AimAtTarget();
        UpdateSpriteFlip();

        _prevPosition = GlobalPosition;
    }

    private void AimAtTarget()
    {
        if (TargetEntity is not null)
        {
            UpdateRotationToTargetEntity(TargetEntity.Position);
        }
        else
        {
            UpdateRotationToRest();
        }
    }

    private void SearchTarget()
    {
        var bodies = GetOverlappingBodies();
        var center = GlobalPosition;
        var minThreshold = MinRange * MinRange;

        var minLen = float.MaxValue;
        var maxLen = float.MinValue;

        Entities.Clear();
        _nearestEntity = null;
        _farthestEntity = null;

        foreach (var o in bodies)
        {
            if (o is not IEntity entity)
            {
                continue;
            }

            if (entity.IsDead)
            {
                continue;
            }

            var distance = center.DistanceSquaredTo(entity.Position);

            // Min Range に満たない場合は無視
            if (distance < minThreshold)
            {
                continue;
            }

            Entities.Add(entity);

            if (distance < minLen)
            {
                minLen = distance;
                _nearestEntity = entity;
            }

            if (distance > maxLen)
            {
                maxLen = distance;
                _farthestEntity = entity;
            }
        }
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

    private void UpdateRotationToRest()
    {
        if (RotateSensitivity <= 0f)
        {
            return;
        }

        Rotation = Mathf.LerpAngle(Rotation, _restAngle, RotateSensitivity);
    }

    private void UpdateRotationToTargetEntity(Vector2 targetPosition)
    {
        if (RotateSensitivity <= 0f)
        {
            return;
        }

        _targetAngle = Mathf.Atan2(targetPosition.Y - GlobalPosition.Y, targetPosition.X - GlobalPosition.X);
        Rotation = Mathf.LerpAngle(Rotation, _targetAngle, RotateSensitivity);
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