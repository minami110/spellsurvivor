using Godot;

namespace fms;

[GlobalClass]
public partial class InfinityGameSettings : FmsResource
{
    [ExportCategory("Wave Settings")]
    [Export]
    public ShopConfig ShopConfig { get; private set; } = null!;

    [Export]
    public BattleWaveConfig WaveConfig { get; private set; } = null!;
}