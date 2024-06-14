using Godot;

namespace fms;

[GlobalClass]
public partial class InfinityGameSettings : FmsResource
{
    [Export(PropertyHint.Range, "0,1000,1")]
    public uint StartMoney { get; private set; } = 10u;

    [ExportCategory("Wave Settings")]
    [Export]
    public ShopConfig ShopConfig { get; private set; } = null!;

    [Export]
    public BattleWaveConfig WaveConfig { get; private set; } = null!;
}