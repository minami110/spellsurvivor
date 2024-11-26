using Godot;

namespace fms;

/// <summary>
/// Minion のデータ Shop 販売
/// </summary>
[GlobalClass]
public partial class MinionCoreData : Resource
{
    [Export]
    public PackedScene WeaponPackedScene { get; private set; } = null!;

    /// <summary>
    /// アイテムのティア
    /// </summary>
    [Export(PropertyHint.Range, "1,5,1")]
    public int Tier { get; private set; } = 1;

    /// <summary>
    /// ショップで購入する際の値段
    /// </summary>
    [Export(PropertyHint.Range, "0,100,1")]
    public uint Price { get; private set; } = 10;

    /// <summary>
    /// ユーザーに表示する名称 (ToDo: 要 Localize)
    /// </summary>
    [ExportGroup("For User Information")]
    [Export]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// ユーザーに表示する武器の説明 (ToDo: 要 Localize)
    /// </summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Shop 画面などで表示するアイコン
    /// </summary>
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