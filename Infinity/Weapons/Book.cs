using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Book : WeaponBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    private protected override void SpawnProjectile(uint level)
    {
        switch (level)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet();
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(100, 2);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(100, 3);
                break;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="spawnCount">たまの数</param>
    private void SpawnBullet(float radius = 100f, int spawnCount = 1)
    {
        for (var i = 0; i < spawnCount; i++)
        {
            var bullet = _bulletPackedScene.Instantiate<RotateBook>();
            {
                bullet.BaseDamage = 50f;
                bullet.Radius = radius;
                bullet.LifeFrame = SolvedCoolDownFrame;
                // 武器の持つクールダウンにあわせて360度回転するような速度を設定する
                // Note: 360度 / クールダウン(フレーム) / 60 フレーム (= かかる秒数)
                bullet.SpeedDegreePerSecond = 360f / (SolvedCoolDownFrame / 60f);
                bullet.OffsetAngleDegree = 360f * i / spawnCount;
            }
            // Note: この Projectile はかってに GlobalPosition を更新するのでこっちで代入する必要はない
            AddChild(bullet);
        }
    }
}