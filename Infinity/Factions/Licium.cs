using System;
using System.Collections.Generic;
using fms.Effect;
using fms.Mob;
using Godot;
using R3;

namespace fms.Faction;

/// <summary>
/// 吸血鬼のシナジー
/// Lv1: 手持ちの Blood Weapon に LifeSteal を付与
/// </summary>
[GlobalClass]
public partial class Licium : FactionBase
{
    // 召喚する コウモリ のパス
    private readonly PackedScene _bat = ResourceLoader.Load<PackedScene>("res://base/entities/bat.tscn");
    private IDisposable? _staticsManagerSubscription;
    public override string MainDescription => "FACTION_LICIUM_DESC_MAIN";

    public override IDictionary<uint, string> LevelDescriptions =>
        new Dictionary<uint, string>
        {
            { 1u, "FACTION_LICIUM_DESC_LEVEL1" },
            { 3u, "FACTION_LICIUM_DESC_LEVEL3" }
        };

    public override void _ExitTree()
    {
        _staticsManagerSubscription?.Dispose();
    }

    private protected override void OnLevelChanged(uint level)
    {
        _staticsManagerSubscription?.Dispose();

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
                {
                    // 30% の確率で発生
                    AddEffectToWeapon(weapon, new Lifesteal { Duration = 0u, Rate = 0.3f });

                    // Licium 武器により敵が死亡したときにコウモリを召喚する
                    _staticsManagerSubscription = StaticsManager.EnemyDamageOccurred
                        .Where(x => x.IsDead)
                        .Subscribe(report =>
                        {
                            if (report.Causer is WeaponBase wpn)
                            {
                                if (wpn.IsBelongTo(FactionType.Licium))
                                {
                                    this.DebugLog("This enemy killed by Licium weapon");
                                    // Spawn Bat
                                    var bat = _bat.Instantiate<Bat>();
                                    {
                                        bat.MoveSpeed = (float)GD.Randfn(100f, 10f);
                                        bat.Damage = 10f;
                                        bat.Lifetime = 240u;
                                        bat.GlobalPosition = report.Position;
                                        var spawnRoot = GetParent().GetParent();
                                        spawnRoot.AddChild(bat);

                                        this.DebugLog($"Bat spawned at {report.Position} in {spawnRoot}");
                                    }
                                }
                            }
                        });

                    break;
                }
                case >= 1:
                {
                    // 10% の確率で発生
                    AddEffectToWeapon(weapon, new Lifesteal { Duration = 0u, Rate = 0.1f });

                    // Licium 武器により敵が死亡したときにコウモリを召喚する
                    _staticsManagerSubscription = StaticsManager.EnemyDamageOccurred
                        .Where(x => x.IsDead)
                        .Subscribe(report =>
                        {
                            if (report.Causer is WeaponBase wpn)
                            {
                                if (wpn.IsBelongTo(FactionType.Licium))
                                {
                                    this.DebugLog("This enemy killed by Licium weapon");
                                    // Spawn Bat
                                    var bat = _bat.Instantiate<Bat>();
                                    {
                                        bat.MoveSpeed = (float)GD.Randfn(100f, 10f);
                                        bat.Damage = 10f;
                                        bat.Lifetime = 240u;
                                        bat.GlobalPosition = report.Position;
                                    }

                                    var world = GetParent().GetParent();
                                    world.CallDeferred(Node.MethodName.AddChild, bat);
                                    GD.Print($"Bat spawned at {report.Position} in {world}");
                                }
                            }
                        });

                    break;
                }
            }
        }
    }
}