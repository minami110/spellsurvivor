using Godot;

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
    }

    private void UpdateRadius()
    {
        _collisionShape.Scale = new Vector2(ItemSettings.Range, ItemSettings.Range);
    }

    private protected override void DoAttack()
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
        var bullet = _bulletPackedScene.Instantiate<ProjectileBase>();
        {
            bullet.Damage = ItemSettings.BaseAttack;
            bullet.Direction = direction;
            bullet.GlobalPosition = GlobalPosition;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }
}