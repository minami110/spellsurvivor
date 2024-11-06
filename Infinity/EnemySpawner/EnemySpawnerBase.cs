using Godot;

namespace fms;

/// <summary>
///     画面端のランダムな位置に Enemy をスポーンする
/// </summary>
public partial class EnemySpawnerBase : Node2D
{
    private protected EnemySpawnConfig? Config { get; set; }

    public void SetConfig(EnemySpawnConfig config)
    {
        Config = config;
    }
}