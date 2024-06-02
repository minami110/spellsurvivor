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
        var weapons = Main.PlayerInventory.Weapons;
        switch (level)
        {
            case >= 6:
            {
                foreach (var weapon in weapons)
                {
                    weapon.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                    weapon.SolveEffect();
                }

                break;
            }
            case >= 4:
            {
                foreach (var weapon in weapons)
                {
                    if (!weapon.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    weapon.AddEffect(new ReduceCoolDownRate { Value = 0.2f });
                    weapon.SolveEffect();
                }

                break;
            }
            case >= 2:
            {
                foreach (var weapon in weapons)
                {
                    if (!weapon.IsFaction<Duelist>())
                    {
                        continue;
                    }

                    weapon.AddEffect(new ReduceCoolDownRate { Value = 0.1f });
                    weapon.SolveEffect();
                }

                break;
            }
        }
    }
}