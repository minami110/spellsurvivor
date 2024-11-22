using Godot;

namespace fms.Faction;

/// <summary>
/// Lv2: Player の最大体力を 50 上げる (100 => 150)
/// Lv4: Player の最大体力 150 上げる (100 => 250)
/// Lv6: Player の最大体力 450 上げる (100 => 500)
/// </summary>
[GlobalClass]
public partial class Bruiser : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        var value = level switch
        {
            >= 6 => 450u,
            >= 4 => 150u,
            >= 2 => 50u,
            _ => 0u
        };

        if (value == 0u)
        {
            return;
        }

        var effect = new Heart { Amount = value };
        AddEffactToPlayer(effect);
    }
}