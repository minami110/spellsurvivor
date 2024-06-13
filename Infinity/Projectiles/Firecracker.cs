using Godot;
using R3;

namespace fms.Projectile;

public partial class Firecracker : ProjectileRigidBodyBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Area2D _enemyDamageArea = null!;

    public Node BulletSpawnNode = null!;

    public FirecrackerSparkData FirecrackerSparkDataSettings;

    public override void _Ready()
    {
        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
    }

    private void OnEnemyBodyEntered(Enemy enemy)
    {
        SpawnFirecrackerSparks();
        KillThis();
    }

    private void SpawnFirecrackerSparks()
    {
        var bullet = _bulletPackedScene.Instantiate<FirecrackerSparks>();
        {
            bullet.GlobalPosition = GlobalPosition;
            bullet.FirecrackerSparkDataSettings = FirecrackerSparkDataSettings;
        }
        BulletSpawnNode.CallDeferred(Node.MethodName.AddChild, bullet);
    }
}

/// <summary>
///     爆竹の火花の初期化用データ
/// </summary>
public readonly struct FirecrackerSparkData
{
    public required int DamageCoolDownFrame { get; init; }
    public required float BaseDamage { get; init; }
    public required float DamageAreaRadius { get; init; }
    public required float LifeFrame { get; init; }
}