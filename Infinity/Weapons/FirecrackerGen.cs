using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
///     爆竹を生成する
/// </summary>
public partial class FirecrackerGen : WeaponBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    [Export]
    private CollisionShape2D _collisionShape = null!;

    [Export]
    private Area2D _searchArea = null!;

    [ExportGroup("Sparks Reference")]
    [Export]
    private int _damageCoolDownFrame = 10; // フレーム毎に一回敵にダメージ
    
    [Export]
    private float _baseDamage = 2; // ダメージ発生1回につき与えるダメージ
    
    [Export]
    private float _damageAreaRadius = 50; // ダメージ範囲の半径 (px)
    
    [Export]
    private float _lifeFrame = 120; // 設定フレーム後に消滅する
    
    public override void _Ready()
    {
        // 範囲 100 px
        _collisionShape.Scale = new Vector2(200, 200);
    }

    private protected override void DoAttack(uint level)
    {
        if (!TryGetNearestEnemy(out var enemy))
        {
            return;
        }

        // Fire to targetEnemy

        // Spawn bullet
        var bullet = _bulletPackedScene.Instantiate<Firecracker>();
        {
            bullet.GlobalPosition = GlobalPosition;
            bullet.BaseDamage = 0;
            var direction = (enemy!.GlobalPosition - GlobalPosition).Normalized();
            bullet.InitialVelocity = direction;
            bullet.InitialSpeed = 500f;
            bullet.BulletSpawnNode = _bulletSpawnNode;
            bullet.FirecrackerSparkDataSettings = new FirecrackerSparkData
            {
                DamageCoolDownFrame = _damageCoolDownFrame,
                BaseDamage = _baseDamage,
                DamageAreaRadius = _damageAreaRadius,
                LifeFrame = _lifeFrame
            };
        }
        _bulletSpawnNode.AddChild(bullet);
    }

    private bool TryGetNearestEnemy(out Enemy? nearestEnemy)
    {
        nearestEnemy = null;

        // Search near enemy
        var overlappingBodies = _searchArea.GetOverlappingBodies();
        if (overlappingBodies.Count <= 0)
        {
            return false;
        }

        // 最も近い敵を検索する
        var distance = 999f;
        foreach (var body in overlappingBodies)
        {
            if (body is Enemy e)
            {
                var d = GlobalPosition.DistanceTo(e.GlobalPosition);
                if (d < distance)
                {
                    distance = d;
                    nearestEnemy = e;
                }
            }
        }

        return nearestEnemy is not null;
    }
}