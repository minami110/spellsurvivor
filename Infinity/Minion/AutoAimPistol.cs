using Godot;
using R3;

namespace fms;

public partial class AutoAimPistol : MinionBase
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

    public override void _Ready()
    {
        base._Ready();

        UpdateRadius();

        Timer.WaitTime = ItemSettings.CoolDown;

        var d1 = Main.GameMode.WaveStarted.Subscribe(_ => Timer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(_ => Timer.Stop());
        var d3 = Timer.TimeoutAsObservable().Subscribe(_ => Fire());

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
            bullet.Damage = ItemSettings.BaseAttack;
            bullet.Direction = direction;
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }

    private void UpdateRadius()
    {
        _collisionShape.Scale = new Vector2(ItemSettings.Range, ItemSettings.Range);
    }
}