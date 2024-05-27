using System;
using Godot;
using R3;

namespace fms;

public partial class AutoAimPistol : Node2D
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    [Export]
    private CollisionShape2D _collisionShape = null!;

    [Export]
    private float _cooldown = 1f;


    private IDisposable _disposables = null!;

    [Export]
    private float _radius = 150f;

    [Export]
    private Area2D _searchArea = null!;

    [Export]
    private Timer _timer = null!;

    public override void _Ready()
    {
        UpdateRadius();

        _timer.WaitTime = _cooldown;

        //
        var d1 = Main.GameMode.WaveStarted.Subscribe(_ => _timer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(_ => _timer.Stop());
        var d3 = _timer.TimeoutAsObservable().Subscribe(_ => Fire());

        _disposables = Disposable.Combine(d1, d2, d3);

        GetTree().DebugCollisionsHint = true;
    }

    private void UpdateRadius()
    {
        _collisionShape.Scale = new Vector2(_radius, _radius);
    }

    public override void _ExitTree()
    {
        _disposables.Dispose();
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

        Vector2 direction;
        if (targetEnemy is not null)
        {
            // Fire to targetEnemy
            direction = (targetEnemy.GlobalPosition - GlobalPosition).Normalized();
        }
        else
        {
            // Fire to forward
            direction = new Vector2(1, 0);
        }

        // Spawn bullet
        var bullet = _bulletPackedScene.Instantiate<Projectile>();
        {
            bullet.Direction = direction;
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }
}