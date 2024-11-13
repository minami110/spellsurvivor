using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// 近くの敵に追従する AreaProjectile
/// </summary>
public partial class ParasiteAreaDamage : AreaProjectile
{
    private State _state = State.SearchEnemy;

    private EnemyBase? _targetEnemy;

    internal uint DamageRadius;

    /// <summary>
    /// 追従速度
    /// </summary>
    internal float FollowSpeed;

    internal uint SearchRadius;

    public override void _EnterTree()
    {
        // ダメージエリアのサイズを設定する
        var col = GetNode<CollisionShape2D>("CollisionShape2D");
        var circle = (CircleShape2D)col.Shape;
        circle.Radius = DamageRadius;

        // 敵検知範囲のサイズを設定する
        var col2 = GetNode<CollisionShape2D>("EnemySearchField/CollisionShape2D");
        var circle2 = (CircleShape2D)col2.Shape;
        circle2.Radius = SearchRadius;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state is State.SearchEnemy or State.FollowParent)
        {
            UpdateTargetAndState();
        }

        switch (_state)
        {
            // 親の位置に戻る
            case State.FollowParent:
            {
                var pos = Weapon.GlobalPosition;
                // 近ければ近いほどゆっくりにする速度バイアスを計算する
                var bias = Mathf.Max(1f - GlobalPosition.DistanceTo(pos) / 50f, 0f);
                ConstantForce = (pos - GlobalPosition).Normalized() * FollowSpeed * (1f - bias);
                break;
            }
            // 敵が死んだら探索状態に戻る
            case State.FollowEnemy when _targetEnemy!.IsDead:
            {
                _state = State.SearchEnemy;
                break;
            }
            // 向きを変更する
            case State.FollowEnemy:
            {
                var pos = _targetEnemy!.GlobalPosition;
                // 近ければ近いほどゆっくりにする速度バイアスを計算する
                var bias = Mathf.Max(1f - GlobalPosition.DistanceTo(pos) / 50f, 0f);
                ConstantForce = (pos - GlobalPosition).Normalized() * FollowSpeed * (1f - bias);
                break;
            }
        }
    }

    private void UpdateTargetAndState()
    {
        // 最も近い敵を検索する
        var distance = float.MaxValue;
        EnemyBase? nearest = null;
        var bodies = GetNode<Area2D>("EnemySearchField").GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is not EnemyBase enemy)
            {
                continue;
            }

            var d = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (d >= distance)
            {
                continue;
            }

            distance = d;
            nearest = enemy;
        }

        // 最も近い追従する敵が見つかった
        if (nearest is not null)
        {
            _state = State.FollowEnemy;
            _targetEnemy = nearest;
        }
        // 敵が見つからなかったらプレイヤーに向かう
        else
        {
            _state = State.FollowParent;
        }
    }

    private enum State
    {
        SearchEnemy,
        FollowParent,
        FollowEnemy
    }
}