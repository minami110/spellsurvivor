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

            switch (level)
            {
                case >= 6:
                {
                    weapon.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                    weapon.SolveEffect();
                    break;
                }
                case >= 4:
                {
                    if (minion.Faction.HasFlag(FactionType.Duelist))
                    {
                        weapon.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                        weapon.SolveEffect();
                    }

                    break;
                }
                case >= 2:
                {
                    if (minion.Faction.HasFlag(FactionType.Duelist))
                    {
                        weapon.AddEffect(new ReduceCoolDownRate { Value = 0.1f });
                        weapon.SolveEffect();
                    }

                    break;
                }
            }
        }
    }
}