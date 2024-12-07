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

    private readonly RandomNumberGenerator _rng = new();
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

        _staticsManagerSubscription?.Dispose();
        _staticsManagerSubscription = level switch
        {
            >= 3 =>
                // Licium 武器により敵が死亡したときにコウモリを召喚する
                StaticsManager.ReportedDamage.Where(x => x.IsVictimDead)
                    .Subscribe(this, (report, self) => self.SpawnBat(report, 0.5f)),
            >= 1 =>
                // Licium 武器により敵が死亡したときにコウモリを召喚する
                StaticsManager.ReportedDamage.Where(x => x.IsVictimDead)
                    .Subscribe(this, (report, self) => self.SpawnBat(report, 0.25f)),
            _ => _staticsManagerSubscription
        };
    }

    private void SpawnBat(DamageReport report, float rate)
    {
        if (report.Causer is not WeaponBase wpn)
        {
            return;
        }

        if (!wpn.IsBelongTo(FactionType.Licium))
        {
            return;
        }

        var choise = _rng.Randf();
        if (choise <= rate)
        {
            return;
        }

        // Spawn Bat
        var bat = _bat.Instantiate<Bat>();
        {
            bat.MoveSpeed = _rng.Randfn(100f, 10f);
            bat.Damage = 10f;
            bat.Lifetime = 240u;
            bat.GlobalPosition = report.Position;
            var world = GetParent().GetParent(); // Note: World/Player/Faction(this)
            world.AddChild(bat);
            this.DebugLog($"Bat spawned at {report.Position} in {world}");
        }
    }
}