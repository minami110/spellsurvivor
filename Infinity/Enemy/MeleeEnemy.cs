using Godot;

namespace fms.Enemy;

/// <summary>
/// 近距離 (接触) ダメージを与える敵のベースクラス
/// </summary>
public partial class MeleeEnemy : EnemyBase
{
    private protected override void Attack()
    {
        var damageArea = GetNode<Area2D>("DamageArea");
        var overlappingBodies = damageArea.GetOverlappingBodies();
        if (overlappingBodies.Count <= 0)
        {
            return;
        }

        foreach (var node in overlappingBodies)
        {
            if (node is IEntity entity)
            {
                entity.ApplayDamage(_power, this, this);
            }
        }
    }
}