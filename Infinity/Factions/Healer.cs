using Godot;

namespace fms.Faction;

/// <summary>
///     Lv2: 敵が 10% の確率で体力を 10 回復するアイテムをドロップする
///     Lv4: 敵が 30% の確率で体力を 10 回復するアイテムをドロップする
///     Lv6: 敵が 50% の確率で体力を 10 回復するアイテムをドロップする
/// </summary>
[GlobalClass]
public partial class Healer : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        var value = level switch
        {
            >= 6 => 50,
            >= 4 => 30,
            >= 2 => 10,
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