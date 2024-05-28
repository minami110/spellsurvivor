using Godot;
using R3;

namespace fms;

public partial class ForwardKnife : Node2D
{
    [Export]
    private float _damage = 50f;

    [Export]
    private float _cooldown = 0.5f;

    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    [Export]
    private Timer _timer = null!;

    public override void _Ready()
    {
        // Set Cooldown
        _timer.WaitTime = _cooldown;

        var d1 = Main.GameMode.WaveStarted.Subscribe(_ => _timer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(_ => _timer.Stop());
        var d3 = _timer.TimeoutAsObservable().Subscribe(_ => Fire());
        Disposable.Combine(d1, d2, d3).AddTo(this);
    }

    private void Fire()
    {
        // Spawn bullet
        var bullet = _bulletPackedScene.Instantiate<Projectile>();
        {
            bullet.Damage = _damage;
            bullet.Direction = GlobalTransform.X; // Forward
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }
}