namespace fms.Faction;

/// <summary>
///     Lv2: Player の最大体力を 50 上げる (100 => 150)
///     Lv4: Player の最大体力 150 上げる (100 => 250)
///     Lv6: Player の最大体力 450 上げる (100 => 500)
/// </summary>
public sealed class Bruiser : FactionBase
{
    private protected override void OnLevelConfirmed(int level)
    {
        var playerState = Main.PlayerState;
        switch (level)
        {
            case >= 6:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 450 });
                playerState.SolveEffect();
                break;
            case >= 4:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 150 });
                playerState.SolveEffect();
                break;
            case >= 2:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 50 });
                playerState.SolveEffect();
                break;
        }
    }

    private protected override void OnLevelReset(int oldLevel)
    {
        var playerState = Main.PlayerState;
        switch (oldLevel)
        {
            case >= 6:
                playerState.AddEffect(new AddMaxHealthEffect { Value = -450 });
                playerState.SolveEffect();
                break;
            case >= 4:
                playerState.AddEffect(new AddMaxHealthEffect { Value = -150 });
                playerState.SolveEffect();
                break;
            case >= 2:
                playerState.AddEffect(new AddMaxHealthEffect { Value = -50 });
                playerState.SolveEffect();
                break;
        }
    }
}