using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using fms.Faction;
using fms.Minion;
using Godot;
using R3;

namespace fms;

public partial class Main : Node
{
    [ExportCategory("Settings")]
    [Export]
    private InfinityGameSettings _gameSettings = null!;

    [ExportGroup("Internal References")]
    [Export]
    private PlayerController _playerController = null!;

    [Export]
    private Node2D _playerPawn = null!;

    [Export]
    private EnemySpawner _enemySpawner = null!;

    private static Main? _instance;

    private readonly Subject<Unit> _changedEquippedMinionSub = new();

    // 現在 Player が装備している Minion の辞書
    private readonly Dictionary<MinionCoreData, MinionBase> _equippedMinions = new();

    // 現在有効な Faction の辞書
    private readonly Dictionary<Type, FactionBase> _factionMap = new();

    private readonly PlayerState _playerState;
    private WaveState _waveState = null!;

    /// <summary>
    ///     Get Main instance
    /// </summary>
    /// <exception cref="ApplicationException"></exception>
    public static Main Instance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _instance ?? throw new ApplicationException("Main instance is null");
    }

    /// <summary>
    ///     Get the current WaveState
    /// </summary>
    public static WaveState WaveState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._waveState;
    }

    /// <summary>
    ///     現在の Player の Global Position を取得
    /// </summary>
    public static Vector2 PlayerGlobalPosition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._playerPawn.GlobalPosition;
    }

    /// <summary>
    ///     Get the PlayerState
    /// </summary>
    public static PlayerState PlayerState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._playerState;
    }

    /// <summary>
    /// </summary>
    public IReadOnlyDictionary<Type, FactionBase> FactionMap => _factionMap;


    /// <summary>
    ///     装備している Minion に変更があった場合に通知
    /// </summary>
    public Observable<Unit> ChangedEquippedMinion => _changedEquippedMinionSub;


    public IReadOnlyDictionary<MinionCoreData, MinionBase> Minions => _equippedMinions;

    public Main()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        else
        {
            throw new AggregateException("Main instance already exists");
        }

        // Create PlayerState
        _playerState = new PlayerState();
    }

    public override void _EnterTree()
    {
        // Note: Main スクリプトは Root 直下に置かれるため, 必ず最初に EnterTree します

        // Create WaveState
        _waveState = new WaveState { Config = _gameSettings.WaveConfig };

        // デバッグ用の Collision を表示
        GetTree().DebugCollisionsHint = true;
    }

    public override void _Ready()
    {
        // PlayerController の初期化
        _playerController.Possess((IPawn)_playerPawn);
        ResetPlayerState();

        // Battle Wave の開始時に Player の体力を全回復する
        var d1 = _waveState.Phase.Where(x => x == WavePhase.BATTLE).Subscribe(this, (_, state) =>
        {
            // Playerの体力を全回復する
            state._playerState.SolveEffect(); // Pending wo kaiketu
            state._playerState.AddEffect(new AddHealthEffect { Value = state._playerState.MaxHealth.CurrentValue });
            state._playerState.SolveEffect();
        });

        // Disposable registration
        Disposable.Combine(_playerState, _waveState, _changedEquippedMinionSub, d1).AddTo(this);
    }

    public override void _Process(double delta)
    {
        _waveState._Process(delta);
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    ///     Minion をショップから購入
    /// </summary>
    /// <param name="minionData"></param>
    public void BuyItem(MinionCoreData minionData)
    {
        // プレイヤーのお金を減らす
        _playerState.AddEffect(new AddMoneyEffect { Value = -minionData.Price });
        _playerState.SolveEffect();

        // すでに Minion を所持している場合
        if (_equippedMinions.TryGetValue(minionData, out var minion))
        {
            // Minion をレベルアップ
            minion.AddEffect(new AddLevelEffect { Value = 1 });
            minion.SolveEffect();
            return;
        }

        // ToDo: 現在デフォで装備にしていますが, 満タンの場合 とか ドラッグで購入とかで色々変わります
        // 装備する
        EquipmentMinion(minionData);
        OnChangedEquippedMinion();
    }


    /// <summary>
    ///     Minion をショップに売却
    /// </summary>
    /// <param name="minionData"></param>
    public void SellItem(MinionCoreData minionData)
    {
        // すでに Minion を所持している場合
        if (!_equippedMinions.TryGetValue(minionData, out var minion))
        {
            return;
        }

        // プレイヤーのお金を増やす
        // TODO: 売却価格を売値と同じにしています
        _playerState.AddEffect(new AddMoneyEffect { Value = minionData.Price });
        _playerState.SolveEffect();

        // Minion を削除
        minion.QueueFree();
        _equippedMinions.Remove(minionData);

        OnChangedEquippedMinion();
    }

    private void EquipmentMinion(MinionCoreData minionData)
    {
        // すでに装備している場合 は何もしない
        if (_equippedMinions.TryGetValue(minionData, out var minion))
        {
            return;
        }

        // Player Pawn に Item を追加で装備させる
        var equipment = minionData.EquipmentScene.Instantiate<MinionBase>();
        equipment.MinionCoreData = minionData;
        _playerPawn.AddChild(equipment);

        // 内部のリストで管理
        _equippedMinions.Add(minionData, equipment);
    }

    private void OnBattleWaveEnded()
    {
        // 湧いた敵をすべてコロス
        GetTree().CallGroup("Enemy", "KillByWaveEnd");
        GetTree().CallGroup("Projectile", "QueueFree");

        // Playerに報酬を与える
        // var reward = _waveSettings[_waveState.Wave.CurrentValue].Money;
        // _playerState.AddEffect(new AddMoneyEffect { Value = reward });
        // _playerState.SolveEffect();
    }

    private void OnChangedEquippedMinion()
    {
        // Faction を一度全て Level 0 に戻す
        foreach (var (_, faction) in _factionMap)
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
                if (_factionMap.TryGetValue(factionType, out var exitingFaction))
                {
                    exitingFaction.AddLevel();
                }
                else
                {
                    _factionMap.Add(factionType, newFaction);
                    newFaction.AddLevel();
                }
            }
        }

        // レベルを確定する 
        foreach (var (_, faction) in _factionMap)
        {
            faction.ConfirmLevel();
        }

        // 通知する
        _changedEquippedMinionSub.OnNext(Unit.Default);
    }

    private void ResetPlayerState()
    {
        // Plauer を初期化する
        _playerState.Reset();
        _playerState.AddEffect(new AddMoveSpeedEffect { Value = _gameSettings.DefaultMoveSpeed });
        _playerState.AddEffect(new AddMoneyEffect { Value = _gameSettings.DefaultMoney });
        _playerState.AddEffect(new AddHealthEffect { Value = _gameSettings.DefaultHealth });
        _playerState.AddEffect(new AddMaxHealthEffect { Value = _gameSettings.DefaultHealth });
        _playerState.SolveEffect();
    }
}