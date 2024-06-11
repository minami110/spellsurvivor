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
    public PackedScene WeaponPackedScene { get; private set; } = null!;

    /// <summary>
    ///     Minion が所有する Faction のリスト (Flag)
    /// </summary>
    [Export(PropertyHint.Flags)]
    public FactionType Faction { get; private set; }

    /// <summary>
    ///     アイテムのティア
    /// </summary>
    [Export(PropertyHint.Range, "1,5,1")]
    public int Tier { get; private set; } = 1;

    /// <summary>
    ///     ショップで購入する際の値段
    /// </summary>
    [Export(PropertyHint.Range, "0,100,1")]
    public uint Price { get; private set; } = 10;

    [ExportGroup("For User Information")]
    [Export]
    public string Name { get; private set; } = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; } = string.Empty;

    [Export]
    public Texture2D Sprite { get; private set; } = null!;

    public string Id
    {
        get
        {
            var path = ResourcePath;
            // Extract file name from path
            var fileName = path.Substring(path.LastIndexOf('/') + 1);
            // Remove extension
            return fileName.Substring(0, fileName.LastIndexOf('.'));
        }
    }
}