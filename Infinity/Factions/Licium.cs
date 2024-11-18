using fms.Effect;
using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
/// 吸血鬼のシナジー
/// Lv1: 手持ちの Blood Weapon に LifeSteal を付与
/// </summary>
[GlobalClass]
public partial class Licium : FactionBase
{
    public override bool IsActiveAnyEffect => Level >= 1u;

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
                    // 30% の確率で 5 ダメージ回復
                    AddEffectToWeapon(weapon, new Lifesteal { Amount = 5u, Rate = 0.3f });
                    break;
                case >= 1:
                    // 10% の確率で 5 ダメージ回復
                    AddEffectToWeapon(weapon, new Lifesteal { Amount = 5u, Rate = 0.1f });
                    break;
            }
        }
    }
}