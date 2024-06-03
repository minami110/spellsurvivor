using Godot;

namespace fms;

[GlobalClass]
public partial class EnemySpawnConfigRaw : FmsResource
{
    [Export]
    public PackedScene EnemyPackedScene { get; private set; } = null!;

    [Export(PropertyHint.Range, "1,99999,1")]
    public int SpawnIntervalFrame { get; private set; } = 120;
}