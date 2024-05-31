using fms.Effect;

namespace fms.Faction;

/// <summary>
///     トリックショットを持つミニオンの通常攻撃が敵にあたった時、範囲内の最も近い敵に向かって反射する
///     Lv2: 跳ね返り回数 1、 跳ね返った攻撃倍率 40 %
///     Lv4: 跳ね返り回数 2、 跳ね返った攻撃倍率 60 %
/// </summary>
public sealed class Trickshot : FactionBase
{
    private protected override void OnLevelConfirmed(int level)
    {
        var minions = Main.GameMode.Minions;
        switch (level)
        {
            case >= 4:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Trickshot>())
                    {
                        continue;
                    }

                    minion.AddEffect(new TrickshotBounce { BounceCount = 2, BounceDamageMultiplier = 0.6f });
                    minion.SolveEffect();
                }

                break;
            }
            case >= 2:
            {
                foreach (var (_, minion) in minions)
                {
                    if (!minion.IsFaction<Trickshot>())
                    {
                        continue;
                    }

                    minion.AddEffect(new TrickshotBounce { BounceCount = 1, BounceDamageMultiplier = 0.4f });
                    minion.SolveEffect();
                }

                break;
            }
        }
    }
}