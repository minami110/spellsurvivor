using Godot;
using R3;

namespace fms;

/// <summary>
/// 画面端のランダムな位置に Enemy をスポーンする
/// </summary>
public partial class EnemySpawnerFromPath : EnemySpawnerBase
{
    [ExportSubgroup("Internal References")]
    [Export]
    private Node _enemySpawnRoot = null!;

    [Export]
    private PathFollow2D _spawnPath = null!;

    // ToDo: 現在メインゲームの Wave のほうが色々仮実装なので, こっち側でスポーンする敵のレベルを管理するカウンターを独自に実装しています
    private uint _enemyLevelCounter = 1u;

    private int _frameCounter;

    public override void _Ready()
    {
        Main.WaveState.Phase.Subscribe(x =>
        {
            if (x == WavePhase.Battle)
            {
                _frameCounter = 0;
                SetProcess(true);
            }
            else if (x == WavePhase.Battleresult)
            {
                SetProcess(false);
                KillAllEnemies();
                _enemyLevelCounter += 1u;
            }
        }).AddTo(this);
    }

    public override void _Process(double delta)
    {
        if (Config == null)
        {
            return;
        }

        _frameCounter++;

        foreach (var c in Config.EnemySpawnConfigRaws)
        {
            if (_frameCounter % c.SpawnIntervalFrame == 0)
            {
                SpawnEnemy(c.EnemyPackedScene, _enemyLevelCounter);
            }
        }
    }

    private void KillAllEnemies()
    {
        GetTree().CallGroup("Enemy", "KillByWaveEnd");
    }

    private void SpawnEnemy(PackedScene packedScene, uint level)
    {
        // pick random point on path
        _spawnPath.ProgressRatio = GD.Randf();
        var samplePosition = _spawnPath.GlobalPosition;

        // Add scene
        var enemy = packedScene.Instantiate<EntityEnemy>();
        enemy.Level = level;

        _enemySpawnRoot.AddChild(enemy);
        enemy.GlobalPosition = samplePosition;
    }
}