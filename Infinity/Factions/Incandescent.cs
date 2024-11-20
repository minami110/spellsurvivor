using fms.Effect;
using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Incandescent
/// </summary>
[GlobalClass]
public partial class Incandescent : FactionBase
{
    // Lv.2 以上で有効な効果がある
    public override bool IsActiveAnyEffect => Level >= 2u;

    private protected override void OnLevelChanged(uint level)
    {
        // Lv2: 兄弟にある Incandescent の Weapon に Strength (+5) を付与
        if (level >= 2u)
        {
            var nodes = this.GetSiblings();
            foreach (var node in nodes)
            {
                if (node is not WeaponBase weapon)
                {
                    continue;
                }

                if (!weapon.IsBelongTo(FactionType.Incandescent))
                {
                    continue;
                }

                AddEffectToWeapon(weapon, new Strength { Amount = 5u });
            }
        }

        // Lv3. プレイヤーに Wing (+10), Dodge (+10%) を付与
        if (level >= 3u)
        {
            AddEffactToPlayer(new Wing { Amount = 10u });
            AddEffactToPlayer(new Dodge { Rate = 0.1f });
        }

        // Lv4. Heat (範囲ダメージ効果) をプレイヤーに付与
        if (level >= 4u)
        {
            AddEffactToPlayer(new Heat { Range = 100u, Span = 20u, Damage = 5u });
        }
    }
}