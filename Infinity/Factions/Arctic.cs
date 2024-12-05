using System;
using System.Collections.Generic;
using fms.Effect;
using Godot;
using R3;

namespace fms.Faction;

/// <summary>
/// https://scrapbox.io/FUMOSurvivor/Arctic
/// </summary>
[GlobalClass]
public partial class Arctic : FactionBase
{
    private readonly HashSet<EffectBase> _publishedLastStandEffects = new();

    private IDisposable? _playerHealthSubscription;
    public override string MainDescription => "FACTION_ARCTIC_DESC_MAIN";

    public override IDictionary<uint, string> LevelDescriptions =>
        new Dictionary<uint, string>
        {
            { 2u, "FACTION_ARCTIC_DESC_LEVEL2" },
            { 4u, "FACTION_ARCTIC_DESC_LEVEL4" }
        };

    public override void _Ready()
    {
        // WaveState の Phase 情報を購読
        var waveState = Main.WaveState;
        waveState.Phase.Subscribe(this, (phase, self) => { self.OnWavePhaseChanged(phase); }).AddTo(this);
    }

    public override void _ExitTree()
    {
        _playerHealthSubscription?.Dispose();
        _playerHealthSubscription = null;
    }

    private protected override void OnLevelChanged(uint level)
    {
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
        else if (level >= 2u)
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
    }

    private void AddLastStandEffectToWeapon(WeaponBase weapon, EffectBase effect)
    {
        // LastStand 用の Effect として登録
        _publishedLastStandEffects.Add(effect);

        // Weapon に Effect を追加
        weapon.State.AddChild(effect);
    }

    // バトル中の Player の HP 監視
    // HP が 半分を切ったら 一度だけ LastStand を発動する
    private void OnChangedOwnerCurrentHealthInBattleWave(float health)
    {
        var maxHealth = OwnerState.Health.MaxValue;
        if (health <= maxHealth * 0.5f)
        {
            if (Level >= 4u)
            {
                foreach (var weapon in SiblingWeapons)
                {
                    AddLastStandEffectToWeapon(weapon, new Strength { Duration = 0u, Rate = 0.2f });
                    AddLastStandEffectToWeapon(weapon, new Haste { Duration = 0u, Amount = 0.4f });
                }

                this.DebugLog("LastStand Lv.2 battle phase effects activated");
            }
            else if (Level >= 2u)
            {
                foreach (var weapon in SiblingWeapons)
                {
                    AddLastStandEffectToWeapon(weapon, new Strength { Duration = 0u, Rate = 0.1f });
                    AddLastStandEffectToWeapon(weapon, new Haste { Duration = 0u, Amount = 0.2f });
                }

                this.DebugLog("LastStand Lv.1 battle phase effects activated");
            }
        }

        var isLastStandEffectPublished = _publishedLastStandEffects.Count > 0;
        if (isLastStandEffectPublished)
        {
            _playerHealthSubscription?.Dispose();
            _playerHealthSubscription = null;
        }
    }

    private void OnWavePhaseChanged(WavePhase phase)
    {
        // Battle Phase に入ったら Player の Health の監視を開始
        if (phase == WavePhase.Battle)
        {
            if (_playerHealthSubscription is not null)
            {
                throw new InvalidProgramException("Subscription is already exists.");
            }

            _playerHealthSubscription = OwnerState.Health.ChangedCurrentValue
                .Subscribe(this, (health, self) => { self.OnChangedOwnerCurrentHealthInBattleWave(health); });
        }
        // Battle Phase が終わったら Player の Health の監視を終了
        else if (phase == WavePhase.Battleresult)
        {
            _playerHealthSubscription?.Dispose();
            _playerHealthSubscription = null;

            // バトル中に LastStand が発動していたら
            if (_publishedLastStandEffects.Count > 0)
            {
                foreach (var effect in _publishedLastStandEffects)
                {
                    if (IsInstanceValid(effect))
                    {
                        effect.QueueFree();
                    }
                }

                _publishedLastStandEffects.Clear();
                this.DebugLog("LastStand battle phase effects deactivated");

                // プレイヤーに永続的に 最大体力を追加する
                // Note: 親で用意された関数を使用すると、自身の Level が変更されたときに消えてしまうので直接追加する
                if (Level >= 4u)
                {
                    OwnerState.AddChild(new Heart { Duration = 0u, Amount = 6u });
                    this.DebugLog("LastStand permanent effects activated");
                }
                else if (Level >= 2u)
                {
                    OwnerState.AddChild(new Heart { Duration = 0u, Amount = 3u });
                    this.DebugLog("LastStand permanent effects activated");
                }
            }
        }
    }
}