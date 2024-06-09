using fms.Effect;

namespace fms.Faction;

/// <summary>
///     すべてのミニオンが、3秒ごとにマナを獲得する
///     Lv2: すべての味方ミニオンに 1マナ
///     Lv4: 「インヴォーカー」は2マナ、他のミニオンは1マナ
///     Lv6: 「インヴォーカー」は3マナ、他のミニオンは2マナ
/// </summary>
public sealed class Invoker : FactionBase
{
    private protected override void OnLevelConfirmed(int level)
    {
        var playerState = Main.PlayerState;

        var minions = Main.PlayerInventory.Minions;
        foreach (var minion in minions)
        {
            if (level >= 2)
            {
                var weapon = minion.Weapon;
                if (weapon == null)
                {
                    continue;
                }
            
                weapon.AddEffect(new AddManaRegeneration { Value = 1, Interval = 180 });
                weapon.SolveEffect();
            }
        }
    }

    private protected override void OnLevelReset(int oldLevel)
    {

        var playerState = Main.PlayerState;

        var minions = Main.PlayerInventory.Minions;
        foreach (var minion in minions)
        {
            if (oldLevel >= 2)
            {
                var weapon = minion.Weapon;
                if (weapon == null)
                {
                    continue;
                }
            
                weapon.AddEffect(new AddManaRegeneration { Value = -1, Interval = -180 });
                weapon.SolveEffect();
            }
        }
    }
}