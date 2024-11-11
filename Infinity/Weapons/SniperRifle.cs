using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class SniperRifle : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.FarthestEnemy;
        var prj = _projectile.Instantiate<BulletProjectile>();

        prj.Damage = 12;
        prj.LifeFrame = 15;
        prj.Speed = 2000;
        prj.PenetrateEnemy = true;
        prj.PenetrateWall = false;

        AddProjectile(prj, GlobalPosition, enemy!.GlobalPosition - GlobalPosition);

        // Note: AddTo はシーンに入れたあとしかできないので
        // Projectile が Enemy にヒットしたら Stack を一つ貯める
        // Prj は貫通するが, 一つの Prj につき 1回だけ Stack を貯められるので Take(1) を入れている
        prj.Hit
            .Where(x => x.HitNode is EnemyBase)
            .Subscribe(prj, (x, s) => { s.Damage *= 0.5f; })
            .AddTo(prj);
    }
}