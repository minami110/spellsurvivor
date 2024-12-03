using Godot;

namespace fms.Faction;

/// <summary>
/// Lv3: ショップフェイズ開始時、スクラップ を持つミニオンのレベルを 1 上げる
/// Lv5: レベルが最大のウェポンの攻撃力を上げる (未実装)
/// </summary>
[GlobalClass]
public partial class Scrap : FactionBase
{
    public override void _Ready()
    {
    }
}