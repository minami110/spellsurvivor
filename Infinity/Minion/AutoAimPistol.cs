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

    private protected override int BaseCoolDownFrame => 120;

    public override void _Ready()
    {
        base._Ready();
        UpdateRadius();
    }

    private protected override void DoAttack()
    {
        if (!IsEnemyInSearchArea(out var enemy))
        {
            return;
        }

        // Fire to targetEnemy
        var direction = (enemy!.GlobalPosition - GlobalPosition).Normalized();

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

    private bool IsEnemyInSearchArea(out Enemy? enemy)
    {
        enemy = null;

        // Search near enemy
        var overlappingBodies = _searchArea.GetOverlappingBodies();
        var distance = 999f;

        if (overlappingBodies.Count <= 0)
        {
            return enemy is null;
        }

        foreach (var body in overlappingBodies)
            if (body is Enemy e)
            {
                var d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (d < distance)
                {
                    distance = d;
                    enemy = e;
                }
            }

        return enemy is null;
    }

    private void UpdateRadius()
    {
        _collisionShape.Scale = new Vector2(ItemSettings.Range, ItemSettings.Range);
    }
}