using Godot;

namespace fms;

public enum BattleMode
{
    Time,
    Annihilation
}

[GlobalClass]
public partial class BattleWaveConfigRaw : FmsResource
{
    [Export]
    public BattleMode Mode { get; private set; } = BattleMode.Time;

    [Export]
    public float WaveTimeSeconds { get; private set; } = 20f;

    [Export]
    public int Reward { get; private set; } = 10;

    [Export]
    public EnemySpawnConfig EnemySpawnConfig { get; private set; } = null!;
}