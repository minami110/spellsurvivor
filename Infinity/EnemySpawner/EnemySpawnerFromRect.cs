using Godot;
using R3;

namespace fms;

/// <summary>
///     画面端のランダムな位置に Enemy をスポーsんする
/// </summary>
public partial class EnemySpawnerFromRect : EnemySpawnerBase
{
    [Export]
    private Vector2I _spawnHalfRectSize = new(500, 500);

    [Export]
    private PackedScene _emepySpawnMarkerPackScene = null!;

    private int _frameCounter;

    // ToDo: 現在メインゲームの Wave のほうが色々仮実装なので, こっち側でスポーンする敵のレベルを管理するカウンターを独自に実装しています
    private uint _enemyLevelCounter = 1u;

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
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    private void SpawnEnemy(PackedScene packedScene, uint level)
    {
        // pick random point from rect
        var samplePosition = new Vector2(
            _spawnHalfRectSize.X * (GD.Randf() - 0.5f) * 2f,
            _spawnHalfRectSize.Y * (GD.Randf() - 0.5f) * 2f
        );

        var marker = _emepySpawnMarkerPackScene.Instantiate<EnemySpawnMarker>();
        {
            marker.GlobalPosition = samplePosition;
            marker.LifeTime = 90;
            marker.EnemyScene = packedScene;
            marker.EnemyLevel = level;
            marker.EnemeySpawnParent = this;
        }
        AddChild(marker);
    }
}