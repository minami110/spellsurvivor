using fms.Weapon;

namespace fms.Faction;

/// <summary>
///     Lv2: デュエリスト を持つミニオンのクールダウンを 10%減少させる
///     Lv4: デュエリスト を持つミニオンのクールダウンを 20%減少させる
///     Lv6: 自分が持っているすべてのミニオンのクールダウンを 20%減少させる
/// </summary>
public sealed class Duelist : FactionBase
{
    private protected override void OnLevelConfirmed(int level)
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

            if (level >= 6)
            {
                AddReduceCoolDownEffect(weapon, 0.2f);
                continue;
            }

            if (minion.Faction.HasFlag(FactionType.Duelist))
            {
                switch (level)
                {
                    case >= 4:
                        AddReduceCoolDownEffect(weapon, 0.2f);
                        break;
                    case >= 2:
                        AddReduceCoolDownEffect(weapon, 0.1f);
                        break;
                }
            }
        }
    }

    private protected override void OnLevelReset(int oldLevel)
    {
        if (oldLevel < 2)
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

            if (oldLevel >= 6)
            {
                AddReduceCoolDownEffect(weapon, -0.2f);
                continue;
            }

            if (minion.Faction.HasFlag(FactionType.Duelist))
            {
                switch (oldLevel)
                {
                    case >= 4:
                        AddReduceCoolDownEffect(weapon, -0.2f);
                        break;
                    case >= 2:
                        AddReduceCoolDownEffect(weapon, -0.1f);
                        break;
                }
            }
        }
    }

    private static void AddReduceCoolDownEffect(WeaponBase weapon, float value)
    {
        weapon.AddEffect(new ReduceCoolDownRate { Value = value });
        weapon.SolveEffect();
    }
}