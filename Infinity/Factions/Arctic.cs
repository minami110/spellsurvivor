using System.Collections.Generic;
using fms.Effect;
using Godot;

namespace fms.Faction;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Arctic
/// </summary>
[GlobalClass]
public partial class Arctic : FactionBase
{
    public override string MainDescription => "FACTION_ARCTIC_DESC_MAIN";

    public override IDictionary<uint, string> LevelDescriptions =>
        new Dictionary<uint, string>
        {
            { 2u, "FACTION_ARCTIC_DESC_LEVEL2" },
            { 4u, "FACTION_ARCTIC_DESC_LEVEL4" }
        };

    private protected override void OnLevelChanged(uint level)
    {
        if (level >= 2u)
        {
            // Added player heart +25
            AddEffactToPlayer(new Heart { Duration = 0u, Amount = 25u });

            // Added 10% strength to arctic weapon
            var nodes = this.GetSiblings();
            foreach (var node in nodes)
            {
                if (node is not WeaponBase weapon)
                {
                    continue;
                }

                if (!weapon.IsBelongTo(FactionType.Arctic))
                {
                    continue;
                }

                AddEffectToWeapon(weapon, new Strength { Duration = 0u, Rate = 0.1f });
            }
        }

        if (level >= 4u)
        {
            // Added player heart +50
            AddEffactToPlayer(new Heart { Duration = 0u, Amount = 50u });

            // Added 15% strength to arctic weapon
            var nodes = this.GetSiblings();
            foreach (var node in nodes)
            {
                if (node is not WeaponBase weapon)
                {
                    continue;
                }

                if (!weapon.IsBelongTo(FactionType.Arctic))
                {
                    continue;
                }

                AddEffectToWeapon(weapon, new Strength { Duration = 0u, Rate = 0.15f });
            }
        }
    }
}