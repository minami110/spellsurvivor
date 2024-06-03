using Godot;
using Godot.Collections;

namespace fms;

[GlobalClass]
public partial class EnemySpawnConfig : FmsResource
{
    [Export]
    public Array<EnemySpawnConfigRaw> EnemySpawnConfigRaws { get; private set; } = new();
}