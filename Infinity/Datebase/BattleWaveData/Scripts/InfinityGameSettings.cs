using Godot;

namespace fms;

[GlobalClass]
public partial class InfinityGameSettings : FmsResource
{
    [ExportGroup("Default Player Settings")]
    [Export(PropertyHint.Range, "0,1000,1")]
    public int DefaultMoney { get; private set; } = 10;

    [Export]
    public float DefaultHealth { get; private set; } = 100f;

    [Export]
    public float DefaultMoveSpeed { get; private set; } = 100f;


    [ExportGroup("Wave Settings")]
    [Export]
    public BattleWaveConfig WaveConfig { get; private set; } = null!;
}