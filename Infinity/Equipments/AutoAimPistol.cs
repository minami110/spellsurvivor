using Godot;
using R3;

namespace fms;

public partial class AutoAimPistol : Node2D
{
    [Export]
    private float _damage = 34f;

    [Export]
    private float _radius = 100f;

    [Export]
    private float _cooldown = 1f;

    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    [Export]
    private CollisionShape2D _collisionShape = null!;

    [Export]
    private Area2D _searchArea = null!;

    [Export]
    private Timer _timer = null!;

    public override void _Ready()
    {
        UpdateRadius();

        _timer.WaitTime = _cooldown;

        var d1 = Main.GameMode.WaveStarted.Subscribe(_ => _timer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(_ => _timer.Stop());
        var d3 = _timer.TimeoutAsObservable().Subscribe(_ => Fire());
        Disposable.Combine(d1, d2, d3).AddTo(this);
    }

    private void Fire()
    {
        // Search near enemy
        var overlappingBodies = _searchArea.GetOverlappingBodies();

        Enemy? targetEnemy = null;
        var distance = 999f;

        if (overlappingBodies.Count > 0)
        {
            foreach (var body in overlappingBodies)
                if (body is Enemy enemy)
                {
                    var d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                    if (d < distance)
                    {
                        distance = d;
                        targetEnemy = enemy;
                    }
                }
        }

        if (targetEnemy is null)
        {
            // 範囲内に敵が居ない場合は打つのに失敗する
            return;
        }

        // Fire to targetEnemy
        var direction = (targetEnemy.GlobalPosition - GlobalPosition).Normalized();

        // Spawn bullet
        var bullet = _bulletPackedScene.Instantiate<Projectile>();
        {
            bullet.Damage = _damage;
            bullet.Direction = direction;
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }

    private void UpdateRadius()
    {
        _collisionShape.Scale = new Vector2(_radius, _radius);
    }
}