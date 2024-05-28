using Godot;

namespace fms;

[GlobalClass]
public partial class WaveSetting : Resource
{
    [Export]
    public int Money;

    [Export]
    public float Time;

    [Export]
    public EnemySpawnSettings[] EnemySpawnSettings = null!;
}