using Godot;

namespace fms;

[GlobalClass]
public partial class EnemySpawnSettings : Resource
{
    [Export]
    public PackedScene EnemyScene = null!;

    [Export]
    public float SpawnInterval = 1f;
}