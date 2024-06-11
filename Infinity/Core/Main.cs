using System;
using System.Runtime.CompilerServices;
using fms.Weapon;
using Godot;
using R3;

namespace fms;

public partial class Main : Node
{
    [ExportCategory("Settings")]
    [Export]
    private InfinityGameSettings _gameSettings = null!;

    private static Main? _instance;

    private PlayerState _playerState = null!;
    private ShopState _shopState = null!;
    private WaveState _waveState = null!;

    public static ShopState Shop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._shopState;
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
    ///     Get Main instance
    /// </summary>
    /// <exception cref="ApplicationException"></exception>
    private static Main Instance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _instance ?? throw new ApplicationException("Main instance is null");
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
    }

    public override void _EnterTree()
    {
        // Initialize States
        _waveState = new WaveState { Config = _gameSettings.WaveConfig };
        _shopState = new ShopState(_gameSettings.ShopConfig);
        AddChild(_shopState);
    }

    public override void _Ready()
    {
        // PlayerState をキャッシュ
        _playerState = (PlayerState)GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayerState);

        // Battle Wave の開始時
        var d1 = _waveState.Phase.Where(x => x == WavePhase.Battle).Subscribe(this, (_, state) =>
        {
            // Playerの体力を全回復する
            state._playerState.AddEffect(new AddHealthEffect { Value = state._playerState.MaxHealth.CurrentValue });

            // Spawner に設定を渡す
            var node = GetTree().GetFirstNodeInGroup("EnemySpawner");
            if (node is EnemySpawnerBase spawner)
            {
                spawner.SetConfig(state._waveState.CurrentWaveConfig.EnemySpawnConfig);
            }

            // すべての武器を起動する
            var nodes = GetTree().GetNodesInGroup(Constant.GroupNameWeapon);
            foreach (var j in nodes)
            {
                if (j is not WeaponBase weapon)
                {
                    continue;
                }

                weapon.StartAttack();
            }

            // BGM のこもりを解消する (150hz => 2000hz (default))
            var tween = CreateTween();
            tween.TweenMethod(Callable.From((float value) => SoundManager.SetBgmBusLowPassFilterCutOff(value)), 150f,
                2000f, 2d);
            tween.Play();
        });

        // Battle Result 進入時
        var d2 = _waveState.Phase.Where(x => x == WavePhase.Battleresult).Subscribe(this, (_, state) =>
        {
            var tree = state.GetTree();

            // すべての武器を停止する
            var nodes = GetTree().GetNodesInGroup(Constant.GroupNameWeapon);
            foreach (var j in nodes)
            {
                if (j is not WeaponBase weapon)
                {
                    continue;
                }

                weapon.StopAttack();
            }

            // 残った Projectile をすべてコロス
            tree.CallGroup("Projectile", "QueueFree");
        });

        // Shop 進入時
        var d3 = _waveState.Phase.Where(x => x == WavePhase.Shop).Subscribe(this, (_, state) =>
        {
            if (state._waveState.Wave.CurrentValue >= 0)
            {
                // Playerに報酬を与える
                var reward = state._waveState.CurrentWaveConfig.Reward;
                state._playerState.AddEffect(new MoneyEffect { Value = reward });
            }
            else
            {
                // PlayerController の初期化
                ResetPlayerState();
            }

            // Shop のリロール
            state._shopState.RefreshInStoreMinions();

            // BGM をこもらせる (2000hz(Default) => 150hz)
            var tween = CreateTween();
            tween.TweenMethod(Callable.From((float value) => SoundManager.SetBgmBusLowPassFilterCutOff(value)), 2000f,
                150f, 0.5d);
            tween.Play();
        });

        // Disposable registration
        Disposable.Combine(_waveState, _shopState, d1, d2, d3).AddTo(this);

        // Start Game
        _waveState.SendSignal(WaveState.Signal.GamemodeStart);
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

    private void ResetPlayerState()
    {
        // Plauer を初期化する
        _playerState.Reset();
        _playerState.AddEffect(new AddMoveSpeedEffect { Value = _gameSettings.DefaultMoveSpeed });
        _playerState.AddEffect(new MoneyEffect { Value = (int)_gameSettings.DefaultMoney });
        _playerState.AddEffect(new AddHealthEffect { Value = _gameSettings.DefaultHealth });
        _playerState.AddEffect(new AddMaxHealthEffect { Value = _gameSettings.DefaultHealth });
    }
}