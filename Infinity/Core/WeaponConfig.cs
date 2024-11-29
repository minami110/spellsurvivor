using System.Text.RegularExpressions;
using fms.Faction;
using Godot;

namespace fms;

/// <summary>
/// <see cref="WeaponBase"/> の Stats を設定するためのリソース
/// </summary>
[GlobalClass]
public partial class WeaponConfig : Resource
{
    /// <summary>
    /// 基礎ダメージ
    /// </summary>
    [ExportGroup("Base Status")]
    [Export(PropertyHint.Range, "0,9999,")]
    public uint Damage { get; private set; } = 10u;

    /// <summary>
    /// 基礎クールダウン
    /// </summary>
    /// <remarks>
    /// バトル中はこのフレーム待機 => アニメーション再生時間 の2つが実際の武器の攻撃頻度となる
    /// </remarks>
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
    public TierType Tier { get; private set; } = TierType.Common;

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

    private string? _id;

    /// <summary>
    /// 武器の Id を取得
    /// </summary>
    /// <remarks>リソースファイルのファイル名が使用されます, 例: Hocho.tres => Hocho </remarks>
    public string Id
    {
        get
        {
            if (_id is not null)
            {
                return _id;
            }

            var path = ResourcePath;
            // Extract file name from path
            var fileName = path.Substring(path.LastIndexOf('/') + 1);
            // Remove extension
            _id = fileName.Substring(0, fileName.LastIndexOf('.'));
            return _id;
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