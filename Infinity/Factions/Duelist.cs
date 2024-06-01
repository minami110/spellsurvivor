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
        var minions = Main.PlayerInventory.EquippedMinions;
        switch (level)
        {
            case >= 6:
            {
                foreach (var (_, minion) in minions)
                {
                    minion.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                    minion.SolveEffect();
                }

                break;
            }
            case >= 4:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    minion.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                    minion.SolveEffect();
                }

                break;
            }
            case >= 2:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    minion.AddEffect(new ReduceCoolDownRate { Value = 0.1f });
                    minion.SolveEffect();
                }

                break;
            }
        }
    }

    private protected override void OnLevelReset(int oldLevel)
    {
        var minions = Main.PlayerInventory.EquippedMinions;
        switch (oldLevel)
        {
            case >= 6:
            {
                foreach (var (_, minion) in minions)
                {
                    minion.AddEffect(new ReduceCoolDownRate { Value = -0.2f });
                    minion.SolveEffect();
                }

                break;
            }
            case >= 4:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    minion.AddEffect(new ReduceCoolDownRate { Value = -0.2f });
                    minion.SolveEffect();
                }

                break;
            }
            case >= 2:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    minion.AddEffect(new ReduceCoolDownRate { Value = -0.1f });
                    minion.SolveEffect();
                }

                break;
            }
        }
    }
}