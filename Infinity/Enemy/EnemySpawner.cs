using System;
using Godot;
using R3;

namespace fms;

public partial class EnemySpawner : Node2D
{
    private DisposableBag _disposableBag;
    private IDisposable _disposableOnExitTree = null!;
    private int _enemyDeadCount;

    [Export]
    private PackedScene _enemyScene = null!;

    private int _enemySpawnedCount;

    [Export]
    private Node _enemySpawnRoot = null!;

    private int _enemyTotalCount;

    [Export]
    private WaveSetting[] _waveSettings = null!;

    public override void _Ready()
    {
        var d1 = Main.GameMode.WaveStarted.Subscribe(
            this,
            (_, state) => state.StartWave()
        );
        var d2 = Main.GameMode.WaveEnded.Subscribe(
            this,
            (_, state) => state.StopWave()
        );
        var d3 = GetNode<Timer>("EnemySpawnTimer")
            .TimeoutAsObservable()
            .Subscribe(
                this,
                (_, s) => s.SpawnEnemy()
            );
        _disposableOnExitTree = Disposable.Combine(d1, d2, d3);
    }

    public override void _ExitTree()
    {
        _disposableOnExitTree.Dispose();
    }

    private void StartWave()
    {
        var currentWave = Main.GameMode.Wave.CurrentValue;
        if (currentWave < 1)
        {
            return;
        }

        if (currentWave > _waveSettings.Length)
        {
            throw new NotImplementedException("WaveSettings is not enough.");
        }

        // Reset Enemy Counter
        _enemySpawnedCount = 0;
        _enemyDeadCount = 0;
        _enemyTotalCount = _waveSettings[currentWave - 1].EnemyCount;
        DebugGUI.CommitText("Spawner/Total", _enemyTotalCount.ToString());

        // Start Timer
        GetNode<Timer>("EnemySpawnTimer").Start();
    }

    private void StopWave()
    {
        // Clean up disposableBag
        _disposableBag.Dispose();
        _disposableBag = new DisposableBag();

        // Stop Timer
        GetNode<Timer>("EnemySpawnTimer").Stop();
        DebugGUI.RemoveKey("Spawner");
    }

    private void OnSpawnEnemyDead(in DeadReason reason)
    {
        _enemyDeadCount += 1;
        DebugGUI.CommitText($"Spawner/DeadBy/{reason.Instigator}", _enemyDeadCount.ToString());

        if (_enemyTotalCount == _enemyDeadCount)
        {
            Main.GameMode.CompleteWave();
        }
    }

    private void SpawnEnemy()
    {
        if (_enemySpawnedCount >= _enemyTotalCount)
        {
            return;
        }

        _enemySpawnedCount++;
        DebugGUI.CommitText("Spawner/Spawned", _enemySpawnedCount.ToString());

        // pick random point on path
        var spawnPoint = GetNode<PathFollow2D>("SpawnPath/SpawnPoint");
        spawnPoint.ProgressRatio = GD.Randf();
        var spawnGlobalPosition = spawnPoint.GlobalPosition;

        // Spawn Enemy
        var enemy = _enemyScene.Instantiate<Enemy>();
        enemy.MoveSpeed = (float)GD.RandRange(20d, 100d);
        enemy.Dead
            .Take(1)
            .Subscribe(reason => OnSpawnEnemyDead(in reason))
            .AddTo(ref _disposableBag);

        // Add scene
        _enemySpawnRoot.AddChild(enemy);
        enemy.GlobalPosition = spawnGlobalPosition;
    }
}