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
    private Node2D _root = null!;

    private protected override void DoAttack(uint level)
    {
        switch (level)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet(_radius);
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(_radius, 2);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(_radius, 3);
                break;
            }
        }
    }


    /// <summary>
    ///     複数の弾を同時に発射するときの初期位置のずれを設定するために利用する時間を計算する
    /// </summary>
    /// <param name="bulletTotalNum">同時に出す弾の数</param>
    /// <param name="bulletNum">何番目の弾か</param>
    /// <param name="coolDown"></param>
    /// <returns></returns>
    private float CalculateRelativePositon(int bulletTotalNum, int bulletNum, float coolDown)
    {
        if (bulletNum == 0)
        {
            return 0f;
        }

        return coolDown / bulletTotalNum * bulletNum;
    }

    /// <summary>
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="num">たまの数</param>
    /// <param name="degree"></param>
    private void SpawnBullet(float radius = 100f, int num = 1, float degree = 0f)
    {
        for (var i = 0; i < num; i++)
        {
            var bullet = _bulletPackedScene.Instantiate<RotateBook>();
            {
                bullet.BaseDamage = 50;
                bullet.Radius = radius;
                bullet.Angle = degree;
                bullet.SecondPerRound = 3;
                bullet.LifeFrame = (int)SolvedCoolDownFrame;
                bullet.InitTimeForRelativePos = CalculateRelativePositon(num, i, SolvedCoolDownFrame / 60f);
            }

            _root.AddChild(bullet);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        GD.Print(this.Mana);
    }
}