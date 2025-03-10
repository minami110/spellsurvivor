using System;
using System.Runtime.CompilerServices;
using Godot;
using R3;

namespace fms;

public partial class Main : Node
{
    [ExportCategory("Settings")]
    [Export]
    private InfinityGameSettings _gameSettings = null!;

    private static Main? _instance;
    private GoldNuggetShop _goldNuggetShop = null!;

    private EntityState _playerState = null!;
    private Shop _shop = null!;
    private WaveState _waveState = null!;

    public static GoldNuggetShop GoldNuggetShop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._goldNuggetShop;
    }

    public static Shop Shop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._shop;
    }

    /// <summary>
    /// Get the current WaveState
    /// </summary>
    public static WaveState WaveState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Instance._waveState;
    }

    /// <summary>
    /// Get Main instance
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
        _waveState = new WaveState { Config = _gameSettings.WaveConfig };
        AddChild(_waveState);
        _shop = new Shop(_gameSettings.ShopConfig);
        AddChild(_shop);
        _goldNuggetShop = new GoldNuggetShop();
        AddChild(_goldNuggetShop);
    }

    public override void _Ready()
    {
        // PlayerState をキャッシュ
        _playerState = (EntityState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);

        // Battle Wave の開始時
        var d1 = _waveState.Phase.Where(x => x == WavePhase.Battle).Subscribe(this, (_, state) =>
        {
            // Playerの体力を全回復する
            state._playerState.ResetToMaxHealth();

            // Spawner に設定を渡す
            var spawner = (EnemySpawnerBase)GetTree().GetFirstNodeInGroup("EnemySpawner");
            spawner.SetConfig(state._waveState.CurrentWaveConfig.EnemySpawnConfig);

            // すべての武器を起動する
            var player = this.GetPlayerNode();
            foreach (var n in player.GetChildren())
            {
                if (n is WeaponBase weapon)
                {
                    weapon.StartAttack();
                }
            }

            // BGM のこもりを解消する (150hz => 2000hz (default))
            var tween = CreateTween();
            tween.TweenMethod(Callable.From((float value) => SoundManager.SetBgmBusLowPassFilterCutOff(value)), 150f,
                2000f, 2d);
        });

        // Battle Result 進入時
        var d2 = _waveState.Phase.Where(x => x == WavePhase.Battleresult).Subscribe(this, (_, state) =>
        {
            var tree = state.GetTree();

            // すべての武器を停止する
            var player = this.GetPlayerNode();
            foreach (var n in player.GetChildren())
            {
                if (n is WeaponBase weapon)
                {
                    weapon.StopAttack();
                }
            }

            // 残った Projectile をすべてコロス
            tree.CallGroup(Constant.GroupNameProjectile, Node.MethodName.QueueFree);

            // 残った Item をすべてコロス
            tree.CallGroup(Constant.GroupNamePickableItem, Node.MethodName.QueueFree);

            // 残った Mob をすべてコロス
            tree.CallGroup(GroupNames.Mob, Node.MethodName.QueueFree);
        });

        // Shop 進入時
        var d3 = _waveState.Phase.Where(x => x == WavePhase.Shop).Subscribe(this, (_, self) =>
        {
            if (self._waveState.Wave.CurrentValue >= 0)
            {
                // 1 .現在の GoldNugget を換金する
                var current = self._playerState.GoldNugget.CurrentValue;
                var next = _goldNuggetShop.ExchangeGoldNugget(current);
                while (next > 0)
                {
                    self._playerState.AddMoney(1);
                    self._playerState.AddGoldNugget((uint)-next);

                    current = self._playerState.GoldNugget.CurrentValue;
                    next = _goldNuggetShop.ExchangeGoldNugget(current);
                }

                // 2. 現在の所持金から利子を発生させる
                // 現在の所持金 の 10% の切り捨て, 最大 5 まで
                // 32 => 3, 48 => 4, 96 => 5
                current = self._playerState.Money.CurrentValue;
                var interest = (uint)Mathf.Min(Mathf.Floor(current * 0.1f), 5.0);
                self._playerState.AddMoney(interest);

                // 3. Wave 報酬を与える
                var reward = self._waveState.CurrentWaveConfig.Reward;
                self._playerState.AddMoney((uint)reward);
            }

            // Shop のリロール
            self._shop.RefreshWeaponCardsFromWave();

            // BGM をこもらせる (2000hz(Default) => 150hz)
            var tween = self.CreateTween();
            tween.TweenMethod(Callable.From((float value) => SoundManager.SetBgmBusLowPassFilterCutOff(value)), 2000f,
                150f, 0.5d);
        });

        // Disposable registration
        Disposable.Combine(d1, d2, d3).AddTo(this);

        // Start Game
        _waveState.SendSignal(WaveState.Signal.GamemodeStart);
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}