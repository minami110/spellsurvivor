using Godot;

namespace spellsurvivor;

[GlobalClass]
public partial class WaveSetting : Resource
{
    [Export] public int EnemyCount;

    [Export] public float SpawnInterval;
}