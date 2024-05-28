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

    [ExportGroup("Wave Settings")]
    [Export]
    private WaveSetting[] _waveSettings = null!;

    [ExportGroup("Internal References")]
    [Export]
    private PlayerController _playerController = null!;

    [Export]
    private Node2D _playerPawn = null!;

    [Export]
    private PlayerState _playerState = null!;

    [Export]
    private EnemySpawner _enemySpawner = null!;

    private static Main? _instance;
    private readonly IDisposable _disposable;
    private readonly List<EnemySpawnTimer> _enemySpawnTimers = new();
    private readonly ReactiveProperty<float> _remainingWaveSecondRp = new();
    private readonly Subject<Unit> _waveEndedSub = new();
    private readonly ReactiveProperty<int> _waveRp = new();
    private readonly Subject<Unit> _waveStartedSub = new();
    private WaveSetting _currentWaveSettings = null!;

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

        _disposable = Disposable.Combine(_waveRp, _remainingWaveSecondRp, _waveEndedSub, _waveStartedSub);
    }

    public override async void _Ready()
    {
        // デバッグ用の Collision を表示
        GetTree().DebugCollisionsHint = true;

        // PlayerController の初期化
        _playerController.Possess((IPawn)_playerPawn);

        // TODO: 1秒後にバトルを開始
        await this.WaitForSecondsAsync(1f);

        ResetPlayerState();
        EnterBattleWave();
    }

    public override void _Process(double delta)
    {
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

    public void EnterBattleWave()
    {
        // 現在の Wave Settings をもらう
        if (_waveRp.Value >= _waveSettings.Length)
        {
            GD.Print("All waves are cleared");
            return;
        }

        // Wave Settings を初期化する
        _currentWaveSettings = _waveSettings[_waveRp.Value];
        _remainingWaveSecondRp.Value = _currentWaveSettings.Time;
        _enemySpawnTimers.Clear();
        foreach (var enemySpawnSettings in _currentWaveSettings.EnemySpawnSettings)
            _enemySpawnTimers.Add(new EnemySpawnTimer
            {
                EnemyScene = enemySpawnSettings.EnemyScene,
                SpawnInterval = enemySpawnSettings.SpawnInterval,
                Timer = enemySpawnSettings.SpawnInterval
            });

        _waveRp.Value++;
        _waveStartedSub.OnNext(Unit.Default);
    }

    public void ExitBattleWave()
    {
        _waveEndedSub.OnNext(Unit.Default);

        // ToDo: リザルト画面を表示する

        // Playerに報酬を与える
        _playerState.AddEffect(new AddMoneyEffect { Value = _currentWaveSettings.Money });
    }

    public void ExitShop()
    {
        EnterBattleWave();
    }


    public static Vector2 GetPlayerGlobalPosition()
    {
        if (_instance is not null)
        {
            return _instance._playerPawn.GlobalPosition;
        }

        GD.PrintErr("Main instance is null");
        return Vector2.Zero;
    }

    public PlayerState GetPlayerState()
    {
        return _playerState;
    }

    private void ResetPlayerState()
    {
        // Plauer を初期化する
        _playerState.AddEffect(new AddMoneyEffect { Value = _startMoney });
        _playerState.AddEffect(new AddHealthEffect { Value = _startHealth });
        _playerState.AddEffect(new AddMaxHealthEffect { Value = _startHealth });
        _playerState.SolveEffect();
    }

    private class EnemySpawnTimer
    {
        public required PackedScene EnemyScene { get; init; }
        public required float SpawnInterval { get; init; }
        public float Timer { get; set; }
    }
}