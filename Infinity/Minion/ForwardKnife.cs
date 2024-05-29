using Godot;

namespace fms;

public partial class ForwardKnife : MinionBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    private void SpawnBullet(float xOffset)
    {
        var bullet = _bulletPackedScene.Instantiate<ProjectileBase>();
        {
            bullet.Damage = ItemSettings.BaseAttack;
            bullet.Direction = GlobalTransform.X; // Forward
            bullet.GlobalPosition = GlobalPosition + GlobalTransform.Y * xOffset; // ちょっと右にずらす
            bullet.InitialSpeed = 1000f;
        }
        _bulletSpawnNode.AddChild(bullet);
    }

    private protected override void DoAttack()
    {
        switch (Level)
        {
            // Level 1
            case 1:
            {
                SpawnBullet(0f);
                break;
            }
            case 2:
            {
                SpawnBullet(10f);
                SpawnBullet(-10f);
                break;
            }
            case 3:
            {
                SpawnBullet(20f);
                SpawnBullet(0f);
                SpawnBullet(-20f);
                break;
            }
        }
    }
}