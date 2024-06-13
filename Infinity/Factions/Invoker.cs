using fms.Effect;
using fms.Weapon;
using Godot;

namespace fms.Faction;

/// <summary>
///     すべてのミニオンが、3秒ごとにマナを獲得する
///     Lv2: すべての味方ミニオンに 1マナ
///     Lv4: 「インヴォーカー」は2マナ、他のミニオンは1マナ
///     Lv6: 「インヴォーカー」は3マナ、他のミニオンは2マナ
/// </summary>
[GlobalClass]
public partial class Invoker : FactionBase
{
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

            if (level >= 2)
            {
                AddEffectToWeapon(weapon, new AddManaRegeneration { Value = 1, Interval = 180 });
            }
        }
    }
}