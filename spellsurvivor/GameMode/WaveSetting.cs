using Godot;

namespace fms;

[GlobalClass]
public partial class WaveSetting : Resource
{
    [Export]
    public int EnemyCount;

    [Export]
    public float SpawnInterval;
}