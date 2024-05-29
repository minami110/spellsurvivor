using Godot;
using R3;

namespace fms;

public partial class ForwardKnife : MinionBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    public override void _Ready()
    {
        base._Ready();

        // Set Cooldown
        Timer.WaitTime = ItemSettings.CoolDown;

        var d1 = Main.GameMode.WaveStarted.Subscribe(_ => Timer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(_ => Timer.Stop());
        var d3 = Timer.TimeoutAsObservable().Subscribe(_ => Fire());
        Disposable.Combine(d1, d2, d3).AddTo(this);
    }

    private void Fire()
    {
        // Spawn bullet
        var bullet = _bulletPackedScene.Instantiate<Projectile>();
        {
            bullet.Damage = ItemSettings.BaseAttack;
            bullet.Direction = GlobalTransform.X; // Forward
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }
}