using fms.Faction;
using Godot;

namespace fms;

/// <summary>
///     Minion のデータ Shop 販売
/// </summary>
[GlobalClass]
public partial class MinionCoreData : Resource
{
    [Export]
    public string Id { get; private set; } = string.Empty;

    [Export(PropertyHint.Range, "1,8,1")]
    public int Tier { get; private set; } = 1;

    /// <summary>
    ///     Minion が所有する Faction のリスト (Flag)
    ///     Note: FactionType を増やしたら PropertyHint の選択項目も増やしてあげてください
    /// </summary>
    [Export(PropertyHint.Flags, "Bruiser,Duelist,Trickshot")]
    public FactionType Faction { get; private set; } = FactionType.None;

    [Export(PropertyHint.Range, "1,100,1")]
    public int Price { get; private set; } = 10;

    [ExportGroup("For User Information")]
    [Export]
    public string Name { get; private set; } = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; } = string.Empty;

    [ExportGroup("Resouce References")]
    [Export]
    public Texture2D Sprite { get; private set; } = null!;

    [Export]
    public PackedScene WeaponPackedScene { get; private set; } = null!;
}