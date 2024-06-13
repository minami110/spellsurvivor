using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
///     Lv2: 敵が 10% の確率で体力を 10 回復するアイテムをドロップする
///     Lv4: 敵が 30% の確率で体力を 10 回復するアイテムをドロップする
///     Lv6: 敵が 50% の確率で体力を 10 回復するアイテムをドロップする
/// </summary>
[GlobalClass]
public partial class Healer : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        var rate = level switch
        {
            >= 6 => 0.5f,
            >= 4 => 0.3f,
            >= 2 => 0.1f,
            _ => 0f
        };

        if (rate <= 0f)
        {
            return;
        }

        // 兄弟にある武器に効果を付与
        foreach (var node in this.GetSiblings())
        {
            if (node is WeaponBase weapon)
            {
                AddEffectToWeapon(weapon, new EnemyDropSmallHeal { Rate = rate });
            }
        }
    }
}