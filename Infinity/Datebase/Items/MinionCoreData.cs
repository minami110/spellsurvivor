using Godot;

namespace fms;

[GlobalClass]
public partial class MinionCoreData : Resource
{
    [Export]
    public string Name = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description = string.Empty;

    [Export]
    public Texture2D Icon = null!;

    [Export]
    public int Price;

    [Export]
    public PackedScene EquipmentScene = null!;
}