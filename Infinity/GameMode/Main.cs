using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using R3;

namespace fms;

public partial class Main : Node
{
    [ExportGroup("Start Player Settings")]
    [Export]
    private int _startMoney = 5;

    [Export]
    private float _startHealth = 100f;

    [Export]
    private float _startSpeed = 100f;

    [ExportGroup("Wave Settings")]
    [Export]
    private WaveSetting[] _waveSettings = Array.Empty<WaveSetting>();

    [ExportGroup("Internal References")]
    [ExportSubgroup("Core")]
    [Export]
    private PlayerController _playerController = null!;

    [Export]
    private Node2D _playerPawn = null!;

    [Export]
    private EnemySpawner _enemySpawner = null!;

    [ExportSubgroup("HUD")]
    [Export]
    private InGameHUD _inGameHud = null!;

    [Export]
    private ShopHUD _shopHud = null!;


    private static Main? _instance;
    private readonly IDisposable _disposable;
    private readonly List<EnemySpawnTimer> _enemySpawnTimers = new();

    // 現在 Player が装備しているアイテム
    private readonly List<ShopItemSettings> _equipments = new();
    private readonly Subject<ShopItemSettings> _equippedItemSub = new();

    // 現在 Player が装備しているアイテム (実態 のほう) の辞書
    private readonly Dictionary<ShopItemSettings, MinionBase> _minions = new();

    private readonly PlayerState _playerState;

    private readonly ReactiveProperty<float> _remainingWaveSecondRp = new();
    private readonly Subject<Unit> _waveEndedSub = new();
    private readonly ReactiveProperty<int> _waveRp = new(-1);
    private readonly Subject<Unit> _waveStartedSub = new();

    private WaveSetting _currentWaveSettings = null!;

    private MainPhase _phase = MainPhase.INIT;

    /// <summary>
    ///     Get Main instance
    /// </summary>
    /// <exception cref="ApplicationException"></exception>
    public static Main GameMode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _instance ?? throw new ApplicationException("Main instance is null");
    }

    /// <summary>
    ///     現在の Wave
    /// </summary>
    public ReadOnlyReactiveProperty<int> Wave => _waveRp;

    /// <summary>
    ///     Battale Wave が開始したとき
    /// </summary>
    public Observable<Unit> WaveStarted => _waveStartedSub;

    /// <summary>
    ///     Battele Wave が終了した時
    /// </summary>
    public Observable<Unit> WaveEnded => _waveEndedSub;

    /// <summary>
    ///     現在の Wave の残り時間
    /// </summary>
    public ReadOnlyReactiveProperty<float> RemainingWaveSecond => _remainingWaveSecondRp;

    /// <summary>
    ///     プレイヤーがアイテムを新しく装備したときに呼ばれる
    /// </summary>
    public Observable<ShopItemSettings> EquippedItem => _equippedItemSub;

    public IReadOnlyList<ShopItemSettings> Equipments => _equipments;

    public IReadOnlyDictionary<ShopItemSettings, MinionBase> Minions => _minions;

    /// <summary>
    ///     現在の Player の Global Position を取得
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    public static Vector2 PlayerGlobalPosition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_instance is not null)
            {
                return _instance._playerPawn.GlobalPosition;
            }

            throw new AggregateException("Main instance is null");
        }
    }

    /// <summary>
    ///     Get the PlayerState
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    public static PlayerState PlayerState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_instance is not null)
            {
                return _instance._playerState;
            }

            throw new AggregateException("Main instance is null");
        }
    }

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
        _disposable = Disposable.Combine(_playerState, _waveRp, _remainingWaveSecondRp, _waveEndedSub, _waveStartedSub,
            _equippedItemSub);
    }

    public override void _Ready()
    {
        // デバッグ用の Collision を表示
        GetTree().DebugCollisionsHint = true;

        // PlayerController の初期化
        _playerController.Possess((IPawn)_playerPawn);

        // HUD をすべて非表示にする
        _inGameHud.Hide();
        _shopHud.Hide();

        // 各種 Subscription
        _shopHud.Closed.Subscribe(_ => CloseShop()).AddTo(this);

        // 開始時点では Shop を開く
        ResetPlayerState();
        OpenShop();
    }

    public override void _Process(double delta)
    {
        if (_phase != MainPhase.BATTLE)
        {
            return;
        }

        if (_remainingWaveSecondRp.Value <= 0f)
        {
            ExitBattleWave();
            return;
        }

        _remainingWaveSecondRp.Value -= (float)delta;

        // 敵をスポーンする
        foreach (var et in _enemySpawnTimers)
        {
            if (et.Timer > 0f)
            {
                et.Timer -= (float)delta;
                continue;
            }

            et.Timer = et.SpawnInterval;
            _enemySpawner.SpawnEnemy(et.EnemyScene.Instantiate<Enemy>());
        }
    }

    public override void _ExitTree()
    {
        _disposable.Dispose();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void BuyItem(ShopItemSettings shopItemSettings)
    {
        // プレイヤーのお金を減らす
        _playerState.AddEffect(new AddMoneyEffect { Value = -shopItemSettings.Price });
        _playerState.SolveEffect();

        // すでに装備を持っていた場合
        if (_minions.TryGetValue(shopItemSettings, out var e))
        {
            // 装備をレベルアップさせる
            e.AddEffect(new AddLevelEffect { Value = 1 });
            e.SolveEffect();
        }
        else
        {
            // Player Pawn に Item を追加で装備させる
            var equipment = shopItemSettings.EquipmentScene.Instantiate<MinionBase>();
            equipment.ItemSettings = shopItemSettings;
            _playerPawn.AddChild(equipment);
            _minions.Add(shopItemSettings, equipment);
            _equipments.Add(shopItemSettings);
            // 通知する
            _equippedItemSub.OnNext(shopItemSettings);
        }
    }

    private void CloseShop()
    {
        _shopHud.Hide();
        EnterBattleWave();
    }

    private void EnterBattleWave()
    {
        // Wave を一つ進める
        // ToDo: Settings が足りない場合はループさせるので永遠に終わらない
        var newWave = _waveRp.Value + 1;
        if (newWave >= _waveSettings.Length)
        {
            newWave = 0;
        }

        // Wave Settings を初期化する
        _currentWaveSettings = _waveSettings[newWave];
        _remainingWaveSecondRp.Value = _currentWaveSettings.Time;
        _enemySpawnTimers.Clear();
        foreach (var enemySpawnSettings in _currentWaveSettings.EnemySpawnSettings)
            _enemySpawnTimers.Add(new EnemySpawnTimer
            {
                EnemyScene = enemySpawnSettings.EnemyScene,
                SpawnInterval = enemySpawnSettings.SpawnInterval,
                Timer = enemySpawnSettings.SpawnInterval
            });


        // Playerの体力を全回復する
        _playerState.AddEffect(new AddHealthEffect { Value = _playerState.MaxHealth.CurrentValue });
        _playerState.SolveEffect();

        // InGame の HUD を表示する
        _inGameHud.Show();
        _phase = MainPhase.BATTLE;
        _waveRp.Value = newWave;
        _waveStartedSub.OnNext(Unit.Default);
    }

    private void ExitBattleWave()
    {
        _waveEndedSub.OnNext(Unit.Default);

        // 湧いた敵をすべてコロス
        GetTree().CallGroup("Enemy", "KillByWaveEnd");

        // ToDo: リザルト画面を表示する

        // InGame HUD を非表示に
        _inGameHud.Hide();

        // Playerに報酬を与える
        _playerState.AddEffect(new AddMoneyEffect { Value = _currentWaveSettings.Money });
        _playerState.SolveEffect();

        // Shop を開く
        OpenShop();
    }

    private void OpenShop()
    {
        _phase = MainPhase.SHOP;
        _shopHud.Show();
        {
            _shopHud.InitialRerollItems();
        }
    }

    private void ResetPlayerState()
    {
        // Plauer を初期化する
        _playerState.Reset();
        _playerState.AddEffect(new AddMoveSpeedEffect { Value = _startSpeed });
        _playerState.AddEffect(new AddMoneyEffect { Value = _startMoney });
        _playerState.AddEffect(new AddHealthEffect { Value = _startHealth });
        _playerState.AddEffect(new AddMaxHealthEffect { Value = _startHealth });
        _playerState.SolveEffect();
    }

    private enum MainPhase
    {
        INIT,
        SHOP,
        BATTLE,
        BATTLE_RESULT,
        DEFEAT
    }

    private class EnemySpawnTimer
    {
        public required PackedScene EnemyScene { get; init; }
        public required float SpawnInterval { get; init; }
        public float Timer { get; set; }
    }
}