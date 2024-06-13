using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Book : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        for (var i = 0; i < level; i++)
        {
            var prj = _projectile.Instantiate<BaseProjectile>();
            {
                // 武器の持つクールダウンに揃える (+2 はちらつき防止)
                prj.LifeFrame = SolvedCoolDownFrame + 2;
                // 武器の持つクールダウンにあわせて360度回転するような速度を設定する
                // Note: 360度 / クールダウン(フレーム) / 60 フレーム (= かかる秒数)
                prj.Speed = (uint)(360f / (SolvedCoolDownFrame / 60f));
            }

            // Orbit Mod を追加
            var player = GetParent<Node2D>();
            var offset = 360f * i / level;
            prj.AddChild(new Orbit { Target = player, Radius = 100, OffsetDeg = offset });

            FrameTimer.AddChild(prj);
        }
    }
}