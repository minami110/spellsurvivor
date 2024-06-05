using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Book : WeaponBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    [Export]
    // Pixel
    private float _radius = 100f;
    
    [Export]
    private Node2D _shaft = null!;
    
    private protected override int BaseCoolDownFrame => 180;

    private protected override void DoAttack()
    {
        switch (MinionLevel)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet(GlobalPosition, _radius, 0f);
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(GlobalPosition, _radius, 0f);
                SpawnBullet(GlobalPosition, _radius, 180f);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(GlobalPosition, _radius, 0f);
                SpawnBullet(GlobalPosition, _radius, 120f);
                SpawnBullet(GlobalPosition, _radius, 240f);
                break;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="degree">中心から時計回りに何度の位置に出るか</param>
    private void SpawnBullet(Vector2 centerPos ,float radius = 100f, float degree = 0f)
    {
        var bullet = _bulletPackedScene.Instantiate<RotateDisk>();
        {
            bullet.BaseDamage = 50;
            bullet.Radius = radius;
            bullet.Angle = degree;
            bullet.SecondPerRound = 3;
        }

        _shaft.AddChild(bullet);
    }
    
    public override void _Process(double delta)
    {
        _shaft.GlobalPosition = Main.PlayerNode.GlobalPosition;
    }
}