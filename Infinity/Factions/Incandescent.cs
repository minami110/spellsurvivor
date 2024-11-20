using Godot;

namespace fms.Faction;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Incandescent
/// </summary>
[GlobalClass]
public partial class Incandescent : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        var value = level switch
        {
            >= 6 => 450,
            >= 4 => 150,
            >= 2 => 50,
            _ => 0
        };

        if (value == 0)
        {
            return;
        }

        var effect = new AddMaxHealthEffect { Value = value };
        AddEffactToPlayer(effect);
    }
}