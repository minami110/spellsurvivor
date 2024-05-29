using Godot;

namespace fms;

public partial class ForwardKnife : MinionBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    private protected override int BaseCoolDownFrame => 60;

    private protected override void DoAttack()
    {
        switch (Level.CurrentValue)
        {
            // Level 1
            case 1:
            {
                SpawnBullet(GlobalPosition);
                break;
            }
            case 2:
            {
                SpawnBullet(GlobalPosition, 10f);
                SpawnBullet(GlobalPosition, -10f);
                break;
            }
            case 3:
            {
                SpawnBullet(GlobalPosition, 20f);
                SpawnBullet(GlobalPosition, 0f, 10f);
                SpawnBullet(GlobalPosition, -20f);
                break;
            }
        }
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        var bullet = _bulletPackedScene.Instantiate<ProjectileBase>();
        {
            bullet.Damage = ItemSettings.BaseAttack;
            bullet.Direction = GlobalTransform.X; // Forward
            bullet.GlobalPosition = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }
}