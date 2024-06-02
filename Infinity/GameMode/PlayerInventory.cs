using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using fms.Faction;
using fms.Minion;
using R3;

namespace fms;

public sealed class PlayerInventory : IDisposable
{
    private readonly Subject<Unit> _equippedMinionChangedSubject = new();
    private readonly Dictionary<Type, FactionBase> _factions = new();
    private readonly List<MinionInInventory> _minions = new();
    private readonly List<MinionBase> _weapons = new();


    /// <summary>
    ///     現在有効な Faction の Map (Key: FacionType, Value: Faction)
    /// </summary>
    public IReadOnlyDictionary<Type, FactionBase> Factions => _factions;

    /// <summary>
    ///     現在 Player が所有している Minion のリスト
    /// </summary>
    public IReadOnlyList<MinionInInventory> Minions => _minions;

    /// <summary>
    ///     現在 Player が装備している 武器 のリスト
    /// </summary>
    public IReadOnlyList<MinionBase> Weapons => _weapons;

    /// <summary>
    /// </summary>
    public Observable<Unit> EquippedMinionChanged => _equippedMinionChangedSubject;

    public bool AddMinion(MinionInInventory minionData)
    {
        if (HasMinion(minionData))
        {
            return false;
        }

        _minions.Add(minionData);
        return true;
    }


    public void EquipMinion(string minionId)
    {
        // 所持している Minion から該当する Minion を取得する
        var minionData = _minions.FirstOrDefault(m => m.Id == minionId);
        if (minionData == null)
        {
            return;
        }

        // Player Pawn に Item を追加で装備させる
        var weapon = minionData.WeaponPackedScene.Instantiate<MinionBase>();
        Main.PlayerNode.AddChild(weapon);

        // 内部のリストで管理
        _weapons.Add(weapon);
        OnChangedWeapons();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasMinion(MinionInInventory minionData)
    {
        return _minions.Any(m => m.Id == minionData.Id);
    }

    public bool RemoveMinion(MinionInInventory minionData)
    {
        if (!HasMinion(minionData))
        {
            return false;
        }

        UnequipMinion(minionData.Id);
        _minions.Remove(minionData);
        return true;
    }

    public void UnequipMinion(string minionId)
    {
        // 所持している Minion から該当する Minion を取得する
        var minionData = _minions.FirstOrDefault(m => m.Id == minionId);
        if (minionData == null)
        {
            return;
        }

        // Player Pawn から Minion を削除する
        foreach (var weapon in _weapons.Where(weapon => weapon.Id == minionId))
        {
            weapon.QueueFree();
            _weapons.Remove(weapon);
            OnChangedWeapons();
            return;
        }
    }

    public bool UpgradeMinion(MinionInInventory minionData)
    {
        var minion = _minions.FirstOrDefault(m => m.Id == minionData.Id);
        if (minion == null)
        {
            return false;
        }

        minion.SetLevel(minion.Level.CurrentValue + 1);
        return true;
    }

    private void OnChangedWeapons()
    {
        // Faction を一度全て Level 0 に戻す
        foreach (var (_, faction) in _factions)
        {
            faction.ResetLevel();
        }

        // 現在しているすべての Minion を照会する
        foreach (var weapon in _weapons)
        {
            // 各 Minion が持っている Faction ごとに
            foreach (var newFaction in weapon.Factions)
            {
                var factionType = newFaction.GetType();
                if (_factions.TryGetValue(factionType, out var exitingFaction))
                {
                    exitingFaction.AddLevel();
                }
                else
                {
                    _factions.Add(factionType, newFaction);
                    newFaction.AddLevel();
                }
            }
        }

        // レベルを確定する 
        foreach (var (_, faction) in _factions)
        {
            faction.ConfirmLevel();
        }

        // 通知する
        _equippedMinionChangedSubject.OnNext(Unit.Default);
    }

    void IDisposable.Dispose()
    {
        _equippedMinionChangedSubject.Dispose();
    }
}