using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Book : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,500,1,suffix:px")]
    private uint _radius = 100;

    [Export(PropertyHint.Range, "0,99,1")]
    private float _speedMultiplier = 1.0f;

    private protected override void OnCoolDownComplete(uint level)
    {
        for (var i = 0; i < level; i++)
        {
            var prj = _projectile.Instantiate<BulletProjectile>();
            {
                // ダメージ, ノックバック を設定する
                prj.Damage = BaseDamage;
                prj.Knockback = Knockback;

                // 武器の持つクールダウンに揃える (+2 はちらつき防止)
                prj.LifeFrame = SolvedCoolDownFrame + 2;

                // 貫通設定
                prj.PenetrateEnemy = true;
                prj.PenetrateWall = true;
            }

            // Orbit Mod を追加

            // 武器の持つクールダウンにあわせて360度回転するような速度を設定する
            // Note: 360度 / クールダウン(フレーム) / 60 フレーム (= かかる秒数)
            var speed = 360f / (SolvedCoolDownFrame / 60f);
            speed *= _speedMultiplier;

            // Loop の i 番目の弾丸の角度を計算する
            var offset = 360f * i / level;

            prj.AddChild(new Orbit { Target = OwnedEntity, Radius = _radius, OffsetDeg = offset, Speed = speed });

            AddProjectile(prj, GlobalPosition);
        }
    }
}