using System.IO;
using System.Linq;
using fms.Effect;
using Godot;
using Heat = fms.Weapon.Heat;

namespace fms.Faction;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Incandescent
/// </summary>
[GlobalClass]
public partial class Incandescent : FactionBase
{
    // Lv4. で装備する Heat のパス
    private string _heatWeaponPath = "res://Infinity/Weapons/(Weapon) Heat.tscn";

    // Lv.2 以上で有効な効果がある
    public override bool IsActiveAnyEffect => Level >= 2u;

    private protected override void OnLevelChanged(uint level)
    {
        // Lv2: 兄弟にある Incandescent の Weapon に Strength (+ 10%) を付与
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

                AddEffectToWeapon(weapon, new Strength { Duration = 0u, Rate = 0.1f });
            }
        }

        // Lv3. プレイヤーに Wing (+10), Dodge (+10%) を付与
        if (level >= 3u)
        {
            AddEffactToPlayer(new Wing { Duration = 0u, Amount = 10u });
            AddEffactToPlayer(new Dodge { Duration = 0u, Rate = 0.1f });
        }

        // Lv4. Heat Lv.1 をプレイヤーに装備
        if (level >= 4u)
        {
            // もうすでに持っている場合は何もしない
            var siblings = this.GetSiblings();
            if (siblings.OfType<Heat>().Any())
            {
                return;
            }

            // まだ持っていない場合は新たに生成してプレイヤーに追加
            var scene = ResourceLoader.Load<PackedScene>(_heatWeaponPath);
            if (scene is null)
            {
                throw new FileNotFoundException($"Heat weapon scene not found: {_heatWeaponPath}");
            }

            var weapon = scene.Instantiate<Heat>();
            weapon.Level = 1u;
            weapon.AutoStart = false;
            CallDeferred(Node.MethodName.AddSibling, weapon); // 親が Busy なことがあるので CallDeferred で追加
        }
    }
}