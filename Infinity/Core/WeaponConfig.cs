using System.Text.RegularExpressions;
using fms.Faction;
using Godot;

namespace fms;

/// <summary>
/// Minion のデータ Shop 販売
/// </summary>
[GlobalClass]
public partial class WeaponConfig : Resource
{
    /// <summary>
    /// </summary>
    [ExportGroup("Base Status")]
    [Export(PropertyHint.Range, "0,10,")]
    public uint Level { get; private set; } = 1u;

    /// <summary>
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,")]
    public uint Damage { get; private set; } = 10u;

    /// <summary>
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,,suffix:frames")]
    public uint Cooldown { get; private set; } = 10u;

    /// <summary>
    /// </summary>
    [Export(PropertyHint.Range, "0,500,0.1,suffix:%")]
    public float CooldownRate { get; private set; } = 100f;

    /// <summary>
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,,suffix:px/s")]
    public uint Knockback { get; private set; } = 20u;

    /// <summary>
    /// アイテムの所属するシナジーのクラス
    /// </summary>
    [Export]
    public FactionType Faction { get; private set; }

    /// <summary>
    /// アイテムのティア
    /// </summary>
    [ExportGroup("Shop Settings")]
    [Export]
    public TierType TierType { get; private set; } = TierType.Common;

    /// <summary>
    /// ショップで購入する際の値段
    /// </summary>
    [Export(PropertyHint.Range, "0,100,1")]
    public uint Price { get; private set; } = 10;

    /// <summary>
    /// Shop 画面などで表示するアイコン
    /// </summary>
    [Export]
    public Texture2D Sprite { get; private set; } = null!;

    [ExportGroup("For User Setings")]
    [Export(PropertyHint.MultilineText)]
    public string Description { get; private set; } = string.Empty;

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

    public string Name
    {
        get
        {
            // FooBar => FOO_BAR
            var idUppder = Regex.Replace(Id, @"(\p{Ll})(\p{Lu})", "$1_$2").ToUpper();
            return $"WEAPON_{idUppder}";
        }
    }

    public string WeaponPackedScenePath => "res://Infinity/Weapons/(Weapon) " + Id + ".tscn";
}