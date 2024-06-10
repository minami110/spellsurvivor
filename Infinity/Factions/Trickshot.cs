using fms.Effect;
using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
///     トリックショットを持つミニオンの通常攻撃が敵にあたった時、範囲内の最も近い敵に向かって反射する
///     Lv2: 跳ね返り回数 1、 跳ね返った攻撃倍率 40 %
///     Lv4: 跳ね返り回数 2、 跳ね返った攻撃倍率 60 %
/// </summary>
[GlobalClass]
public partial class Trickshot : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        if (level < 2)
        {
            return;
        }

        var minions = Main.PlayerInventory.Minions;
        foreach (var minion in minions)
        {
            // Weapon を所持していない (手札にない)
            var weapon = minion.Weapon;
            if (weapon == null)
            {
                continue;
            }

            // トリックショットを持っていない
            if (!minion.Faction.HasFlag(FactionType.Trickshot))
            {
                continue;
            }

            switch (level)
            {
                case >= 4:
                {
                    AddTrickshotEffect(weapon, 2, 0.6f);
                    break;
                }
                case >= 2:
                {
                    AddTrickshotEffect(weapon, 1, 0.4f);
                    break;
                }
            }
        }
    }

    private static void AddTrickshotEffect(WeaponBase weapon, int bounceCount, float bounceDamageMultiplier)
    {
        weapon.AddEffect(new TrickshotBounce
            { BounceCount = bounceCount, BounceDamageMultiplier = bounceDamageMultiplier });
        weapon.SolveEffect();
    }
}