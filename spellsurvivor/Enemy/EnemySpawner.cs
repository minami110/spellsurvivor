using System;
using System.Diagnostics;
using Godot;
using R3;

namespace spellsurvivor;

public partial class EnemySpawner : Node2D
{
    private IDisposable _disposableOnExitTree = null!;
    [Export] private PackedScene _enemyScene = null!;
    
    private int _enemySpawnCount = 0;
    private int _enemyDeadCount = 0;

    public override void _Ready()
    {
        var timer = GetNode<Timer>("EnemySpawnTimer");
        var d1 = Main.GameMode.WaveStarted.Subscribe(this, (_, state) => { state.StartWave(); });
        var d2 = Main.GameMode.WaveEnded.Subscribe(timer, (_, t) => { t.Stop(); });
        var d3 = timer.TimeoutAsObservable().Subscribe(this, (_, s) => s.SpawnEnemy());
        _disposableOnExitTree = Disposable.Combine(d1, d2, d3);
    }

    public override void _ExitTree()
    {
        _disposableOnExitTree.Dispose();
    }

    private void StartWave()
    {
        // Start Timer
        GetNode<Timer>("EnemySpawnTimer").Start();
        
        // Reset Enemy Counter
        _enemySpawnCount = 0;
        _enemyDeadCount = 0;
    }
    
    private void AddEnemySpawnCount(int count = 1)
    {
        _enemySpawnCount += count;
        DebugGUI.CommitText("EnemySpawnCount", _enemySpawnCount.ToString());
    }
    
    private void AddEnemyDeadCount()
    {
        _enemyDeadCount += 1;
        DebugGUI.CommitText("EnemyDeadCount", _enemyDeadCount.ToString());
        
        if (_enemySpawnCount == _enemyDeadCount)
        {
            Main.GameMode.CompleteWave();
        }
    }

    private void SpawnEnemy()
    {
        if (Main.GameMode.Wave.CurrentValue == 1)
        {
            // First wave has only 10 enemy
            if (_enemySpawnCount >= 1)
            {
                return;
            }
        }
        
        // pick random point on path
        var spawnPoint = GetNode<PathFollow2D>("SpawnPath/SpawnPoint");
        spawnPoint.ProgressRatio = GD.Randf();
        var spawnGlobalPosition = spawnPoint.GlobalPosition;

        // Spawn Enemy
        var enemy = _enemyScene.Instantiate<Enemy>();
        enemy.MoveSpeed = (float)GD.RandRange(20d, 100d);
        enemy.TreeExitedAsObservable().Take(1).Subscribe(_ => AddEnemyDeadCount());

        // Add scene
        GetTree().Root.AddChild(enemy);
        enemy.GlobalPosition = spawnGlobalPosition;
        
        AddEnemySpawnCount();
    }
}