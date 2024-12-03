using System.Collections.Generic;
using fms.Effect;
using Godot;

namespace fms.Faction;

/// <summary>
/// 吸血鬼のシナジー
/// Lv1: 手持ちの Blood Weapon に LifeSteal を付与
/// </summary>
[GlobalClass]
public partial class Licium : FactionBase
{
    public override string MainDescription => "FACTION_LICIUM_DESC_MAIN";

    public override IDictionary<uint, string> LevelDescriptions =>
        new Dictionary<uint, string>
        {
            { 1u, "FACTION_LICIUM_DESC_LEVEL1" },
            { 3u, "FACTION_LICIUM_DESC_LEVEL3" }
        };

    private protected override void OnLevelChanged(uint level)
    {
        // 兄弟にある Weapon にアクセスする
        var nodes = this.GetSiblings();
        foreach (var node in nodes)
        {
            if (node is not WeaponBase weapon)
            {
                continue;
            }

            if (!weapon.IsBelongTo(FactionType.Licium))
            {
                continue;
            }

            switch (level)
            {
                case >= 3:
                    // 30% の確率で発生
                    AddEffectToWeapon(weapon, new Lifesteal { Duration = 0u, Rate = 0.3f });
                    break;
                case >= 1:
                    // 10% の確率で発生
                    AddEffectToWeapon(weapon, new Lifesteal { Duration = 0u, Rate = 0.1f });
                    break;
            }
        }
    }
}