using Godot;
using Godot.Collections;

namespace fms;

[GlobalClass]
public partial class ShopConfig : FmsResource
{
    [Export]
    public Array<MinionCoreData> Tier1DefaultPool { get; private set; } = new();

    [Export]
    public Array<MinionCoreData> Tier2DefaultPool { get; private set; } = new();

    [Export]
    public Array<MinionCoreData> Tier3DefaultPool { get; private set; } = new();
}