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
        var rate = level switch
        {
            >= 6 => 0.5f,
            >= 4 => 0.3f,
            >= 2 => 0.1f,
            _ => 0f
        };

        var p = PickableItemSpawner.Instance;
        if (p is not null)
        {
            p.HeartSpawnRate = rate;
        }
    }
}