using Godot;

namespace fms;

[GlobalClass]
public partial class InfinityGameSettings : FmsResource
{
    [ExportCategory("Default Player Settings")]
    [Export(PropertyHint.Range, "0,1000,1")]
    public uint DefaultMoney { get; private set; } = 10u;

    [Export]
    public float DefaultHealth { get; private set; } = 100f;

    [Export]
    public float DefaultMoveSpeed { get; private set; } = 100f;


    [ExportCategory("Wave Settings")]
    [Export]
    public ShopConfig ShopConfig { get; private set; } = null!;

    [Export]
    public BattleWaveConfig WaveConfig { get; private set; } = null!;
}