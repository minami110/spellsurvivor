using Godot;

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

    public void SpawnEnemy(Enemy enemy)
    {
        // pick random point on path
        _spawnPath.ProgressRatio = GD.Randf();
        var samplePosition = _spawnPath.GlobalPosition;

        // Add scene
        _enemySpawnRoot.AddChild(enemy);
        enemy.GlobalPosition = samplePosition;
    }
}