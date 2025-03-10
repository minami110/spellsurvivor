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

    private protected override void OnCoolDownCompleted(uint level)
    {
        for (var i = 0; i < level; i++)
        {
            var factory = new BulletProjectileFactory
            {
                Instigator = OwnedEntity,
                Causer = this,
                CauserPath = CauserPath,
                Damage = State.Damage.CurrentValue,
                Knockback = State.Knockback.CurrentValue,
                // 武器の持つクールダウンに揃える (+2 はちらつき防止)
                Lifetime = State.AttackSpeed.CurrentValue + 2,
                PenetrateSettings = BulletProjectile.PenetrateType.Enemy | BulletProjectile.PenetrateType.Wall,
                Position = GlobalPosition
            };

            var prj = factory.Create(_projectile);

            // Orbit Mod を追加
            // 武器の持つクールダウンにあわせて360度回転するような速度を設定する
            // Note: 360度 / クールダウン(フレーム) / 60 フレーム (= かかる秒数)
            var speed = 360f / (State.AttackSpeed.CurrentValue / 60f);
            speed *= _speedMultiplier;

            // Loop の i 番目の弾丸の角度を計算する
            var offset = 360f * i / level;

            prj.AddChild(
                new Orbit { Target = (Node2D)OwnedEntity, Radius = _radius, OffsetDeg = offset, Speed = speed });

            AddProjectile(prj);
        }

        RestartCoolDown();
    }
}