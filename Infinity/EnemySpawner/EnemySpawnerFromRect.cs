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

    private void KillAllEnemies()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    private void SpawnEnemy(PackedScene packedScene)
    {
        // pick random point from rect
        var samplePosition = new Vector2(
            _spawnHalfRectSize.X * (GD.Randf() - 0.5f) * 2f,
            _spawnHalfRectSize.Y * (GD.Randf() - 0.5f) * 2f
        );

        var marker = _emepySpawnMarkerPackScene.Instantiate<EnemySpawnMarker>();
        {
            marker.GlobalPosition = samplePosition;
            marker.EnemyScene = packedScene;
            marker.LifeTime = 90;
            marker.EnemeySpawnParent = this;
        }
        AddChild(marker);
    }
}