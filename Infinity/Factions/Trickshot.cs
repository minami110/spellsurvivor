using fms.Effect;
using Godot;

namespace fms.Faction;

/// <summary>
/// トリックショットを持つミニオンの通常攻撃が敵にあたった時、範囲内の最も近い敵に向かって反射する
/// Lv2: 跳ね返り回数 1、 跳ね返った攻撃倍率 40 %
/// Lv4: 跳ね返り回数 2、 跳ね返った攻撃倍率 60 %
/// </summary>
[GlobalClass]
public partial class Trickshot : FactionBase
{
    private protected override void OnLevelChanged(uint level)
    {
        if (level < 2)
        {
            return;
        }

        // 兄弟にある Weapon にアクセスする
        var nodes = this.GetSiblings();
        foreach (var node in nodes)
        {
            if (node is not WeaponBase weapon)
            {
                continue;
            }

            // トリックショットを持っていない
            if (!weapon.IsBelongTo(FactionType.Trickshot))
            {
                continue;
            }

            switch (level)
            {
                case >= 4:
                {
                    AddTrickshotEffect(weapon, 2, 0.6f);
                    break;
                }
                case >= 2:
                {
                    AddTrickshotEffect(weapon, 1, 0.4f);
                    break;
                }
            }
        }
    }

    private void AddTrickshotEffect(WeaponBase weapon, int bounceCount, float bounceDamageMultiplier)
    {
        AddEffectToWeapon(weapon, new TrickshotBounce
        {
            Duration = 0u,
            BounceCount = (uint)bounceCount,
            BounceDamageRate = bounceDamageMultiplier
        });
    }
}