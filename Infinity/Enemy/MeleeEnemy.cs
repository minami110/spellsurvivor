using Godot;

namespace fms.Enemy;

/// <summary>
/// 遠距離からプレイヤーに射撃を行う敵
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
            if (node is BasePlayerPawn player)
            {
                player.TakeDamage(_power, this);
            }
        }
    }
}