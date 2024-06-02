using Godot;

namespace fms;

/// <summary>
///     Minion のデータ Shop 販売
/// </summary>
[GlobalClass]
public partial class MinionCoreData : Resource
{
    [ExportGroup("Basic Information")]
    [Export]
    public string Id = string.Empty;

    [Export]
    public string Name = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description = string.Empty;

    [Export]
    public Texture2D Sprite = null!;

    [Export(PropertyHint.Range, "1,8,1")]
    public int Tier = 1;

    [Export(PropertyHint.Range, "1,100,1")]
    public int Price = 10;

    [ExportGroup("Packed Scene References")]
    [Export]
    public PackedScene WeaponPackedScene = null!;
}