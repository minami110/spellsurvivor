using Godot;
using R3;

namespace fms;

/// <summary>
///     画面端のランダムな位置に Enemy をスポーsんする
/// </summary>
public partial class EnemySpawner : Node2D
{
    [ExportSubgroup("Internal References")]
    [Export]
    private Node _enemySpawnRoot = null!;

    [Export]
    private PathFollow2D _spawnPath = null!;

    private int _frameCounter;

    public EnemySpawnConfig? Config { get; set; }

    public override void _Ready()
    {
        Main.WaveState.Phase.Subscribe(x =>
        {
            if (x == WavePhase.BATTLE)
            {
                _frameCounter = 0;
                SetProcess(true);
            }
            else if (x == WavePhase.BATTLERESULT)
            {
                SetProcess(false);
                KillAllEnemies();
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
                SpawnEnemy(c.EnemyPackedScene);
            }
        }
    }

    public void KillAllEnemies()
    {
        GetTree().CallGroup("Enemy", "KillByWaveEnd");
    }

    public void SpawnEnemy(PackedScene packedScene)
    {
        // pick random point on path
        _spawnPath.ProgressRatio = GD.Randf();
        var samplePosition = _spawnPath.GlobalPosition;

        // Add scene
        var enemy = packedScene.Instantiate<Enemy>();
        _enemySpawnRoot.AddChild(enemy);
        enemy.GlobalPosition = samplePosition;
    }
}