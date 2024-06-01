using System;
using System.Collections.Generic;
using fms.Faction;
using fms.Minion;
using R3;

namespace fms;

public sealed class PlayerInventory : IDisposable
{
    private readonly Subject<Unit> _equippedMinionChangedSubject = new();
    private readonly Dictionary<MinionCoreData, MinionBase> _equippedMinions = new();
    private readonly Dictionary<Type, FactionBase> _factions = new();
    private readonly HashSet<MinionCoreData> _ownedMinions = new();

    /// <summary>
    /// </summary>
    public IReadOnlyDictionary<Type, FactionBase> Factions => _factions;

    public IReadOnlyDictionary<MinionCoreData, MinionBase> EquippedMinions => _equippedMinions;
    public IReadOnlySet<MinionCoreData> OwnMinions => _ownedMinions;

    public Observable<Unit> EquippedMinionChanged => _equippedMinionChangedSubject;

    public bool AddMinion(MinionCoreData minionData)
    {
        if (_ownedMinions.Contains(minionData))
        {
            return false;
        }

        _ownedMinions.Add(minionData);
        return true;
    }

    public void EquipMinion(MinionCoreData minionData)
    {
        // すでに装備している場合 は何もしない
        if (_equippedMinions.ContainsKey(minionData))
        {
            return;
        }

        // Player Pawn に Item を追加で装備させる
        var minion = minionData.EquipmentScene.Instantiate<MinionBase>();
        minion.MinionCoreData = minionData;
        Main.PlayerNode.AddChild(minion);

        // 内部のリストで管理
        _equippedMinions.Add(minionData, minion);

        OnChangedEquippedMinion();
    }

    public bool RemoveMinion(MinionCoreData minionData)
    {
        if (!_ownedMinions.Contains(minionData))
        {
            return false;
        }

        UnequipMinion(minionData);
        _ownedMinions.Remove(minionData);
        return true;
    }

    public bool TryGetEquippedMinion(MinionCoreData minionData, out MinionBase? minion)
    {
        return _equippedMinions.TryGetValue(minionData, out minion);
    }

    public void UnequipMinion(MinionCoreData minionData)
    {
        // 装備してない場合 は何もしない
        if (!_equippedMinions.ContainsKey(minionData))
        {
            return;
        }

        // Player Pawn から Minion を削除する
        var minion = _equippedMinions[minionData];
        minion.QueueFree();
        _equippedMinions.Remove(minionData);

        OnChangedEquippedMinion();
    }

    private void OnChangedEquippedMinion()
    {
        // Faction を一度全て Level 0 に戻す
        foreach (var (_, faction) in _factions)
        {
            faction.ResetLevel();
        }

        // 現在しているすべての Minion を照会する
        foreach (var (_, m) in _equippedMinions)
        {
            // 各 Minion が持っている Faction ごとに
            foreach (var newFaction in m.Factions)
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