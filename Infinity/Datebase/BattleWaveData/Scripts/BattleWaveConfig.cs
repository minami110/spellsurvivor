using Godot;
using Godot.Collections;

namespace fms;

[GlobalClass]
public partial class BattleWaveConfig : FmsResource
{
    [Export]
    public Array<BattleWaveConfigRaw> Waves { get; private set; } = new();

    public int WaveCount => Waves.Count;
}