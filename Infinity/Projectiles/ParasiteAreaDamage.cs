using fms.Projectile;
using Godot;

namespace fms.Weapon;


public partial class ParasiteAreaDamage : AreaProjectile
{
    private enum State
    {
        SearchEnemy,
        FollowPlayer,
        FollowEnemy,
    }

    private State _state = State.SearchEnemy;

    private EnemyBase? _targetEnemy = null;

    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.SearchEnemy || _state == State.FollowPlayer)
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
                if (d < distance)
                {
                    distance = d;
                    nearest = enemy;
                }
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
                _state = State.FollowPlayer;
            }

        }

        if (_state == State.FollowPlayer)
        {
            // Weapon の位置に戻る
            var pos = Weapon.GlobalPosition;
            // 向きを変更する
            Direction = (pos - GlobalPosition).Normalized();

        }
        // 敵追従
        else if (_state == State.FollowEnemy)
        {
            // 敵がまた生きているかどうかを確認する
            // ToDo: 専用の関数用意したほうがいい
            if (IsInstanceValid(_targetEnemy!) == false)
            {
                _state = State.SearchEnemy;
                return;
            }

            // 向きを変更する
            Direction = (_targetEnemy!.GlobalPosition - GlobalPosition).Normalized();
        }
    }
}