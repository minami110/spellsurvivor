using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using fms.Faction;
using fms.Weapon;
using R3;

namespace fms;

public sealed class PlayerInventory : IDisposable
{
    private readonly Dictionary<FactionType, FactionBase> _factions = new();
    private readonly Subject<Unit> _inHandMinionChangedSubject = new();
    private readonly List<MinionInRuntime> _minions = new();

    /// <summary>
    ///     現在有効な Faction の Map (Key: FacionType, Value: Faction)
    /// </summary>
    public IReadOnlyDictionary<FactionType, FactionBase> Factions => _factions;

    /// <summary>
    ///     現在 Player が所有している Minion のリスト
    /// </summary>
    public IReadOnlyList<MinionInRuntime> Minions => _minions;

    /// <summary>
    /// </summary>
    public Observable<Unit> InHandMinionChanged => _inHandMinionChangedSubject;

    public bool AddMinion(MinionInRuntime minion)
    {
        if (HasMinion(minion))
        {
            return false;
        }

        _minions.Add(minion);
        minion.Place = MinionPlace.InStorage;

        return true;
    }

    public void EquipMinion(string minionId)
    {
        // 所持している Minion から該当する Minion を取得する
        var minion = _minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null)
        {
            throw new ApplicationException($"Does not have minion: {minionId}");
        }

        // すでに装備されている
        if (minion.Place == MinionPlace.InHand)
        {
            throw new ApplicationException($"Minion already has been in-hand : {minionId}");
        }

        // Player Pawn に Weapon を追加で装備させる
        minion.Place = MinionPlace.InHand;
        var weapon = minion.WeaponPackedScene.Instantiate<WeaponBase>();
        minion.Weapon = weapon;
        Main.PlayerNode.AddChild(weapon);

        OnEquipedMinionChanged();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasMinion(MinionInRuntime minionData)
    {
        return _minions.Any(m => m.Id == minionData.Id);
    }

    public bool RemoveMinion(MinionInRuntime minion)
    {
        if (!HasMinion(minion))
        {
            return false;
        }

        UnequipMinion(minion.Id);
        _minions.Remove(minion);

        // Reset Minion Status
        minion.ResetRuntimeStatus();
        return true;
    }

    public void UnequipMinion(string minionId)
    {
        // 所持している Minion から該当する Minion を取得する
        var minion = _minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null)
        {
            return;
        }

        if (minion.Place != MinionPlace.InHand)
        {
            return;
        }

        minion.Place = MinionPlace.InStorage;

        var weapon = minion.Weapon;
        if (weapon == null)
        {
            throw new ApplicationException("Minion does not have weapon");
        }

        minion.Weapon = null;
        weapon.QueueFree();
        OnEquipedMinionChanged();
    }

    public bool UpgradeMinion(MinionInRuntime minionData)
    {
        var minion = _minions.FirstOrDefault(m => m.Id == minionData.Id);
        if (minion == null)
        {
            return false;
        }

        minion.SetLevel(minion.Level.CurrentValue + 1);
        return true;
    }

    private void CreateOrUpgradeFaction(FactionType faction)
    {
        if (_factions.TryGetValue(faction, out var existingFaction))
        {
            existingFaction.UpgradeLevel();
        }
        else
        {
            var newFaction = FactionUtil.CreateFaction(faction);
            newFaction.UpgradeLevel(); // Lv.0 => Lv.1
            _factions.Add(faction, newFaction);
        }
    }

    private void OnEquipedMinionChanged()
    {
        // Faction を一度全て Level 0 に戻す
        foreach (var (_, faction) in _factions)
        {
            faction.ResetLevel();
        }

        // 現在しているすべての Minion を照会する
        foreach (var minion in _minions)
        {
            if (minion.Place != MinionPlace.InHand)
            {
                continue;
            }

            foreach (var faction in FactionUtil.GetFactionTypes())
            {
                if (minion.IsBelongTo(faction))
                {
                    CreateOrUpgradeFaction(faction);
                }
            }
        }

        // レベルを確定する 
        foreach (var (_, faction) in _factions)
        {
            faction.ConfirmLevel();
        }

        // 通知する
        _inHandMinionChangedSubject.OnNext(Unit.Default);
    }

    void IDisposable.Dispose()
    {
        _inHandMinionChangedSubject.Dispose();
    }
}