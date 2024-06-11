using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
///     Lv2: デュエリスト を持つミニオンのクールダウンを 10%減少させる
///     Lv4: デュエリスト を持つミニオンのクールダウンを 20%減少させる
///     Lv6: 自分が持っているすべてのミニオンのクールダウンを 20%減少させる
/// </summary>
[GlobalClass]
public partial class Duelist : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        if (level < 2)
        {
            return;
        }

        // 兄弟にある Weapon にアクセスする
        var nodes = this.GetSibling();
        foreach (var node in nodes)
        {
            if (node is not WeaponBase weapon)
            {
                continue;
            }

            if (level >= 6)
            {
                AddReduceCoolDownEffect(weapon, 0.2f);
                continue;
            }

            if (weapon.IsBelongTo(FactionType.Duelist))
            {
                switch (level)
                {
                    case >= 4:
                        AddReduceCoolDownEffect(weapon, 0.2f);
                        break;
                    case >= 2:
                        AddReduceCoolDownEffect(weapon, 0.1f);
                        break;
                }
            }
        }
    }

    private void AddReduceCoolDownEffect(WeaponBase weapon, float value)
    {
        AddEffectToWeapon(weapon, new ReduceCoolDownRate { Value = value });
    }
}