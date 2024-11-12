using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class SniperRifle : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 2000f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 15u;

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 500f;

    public override void _Ready()
    {
        GetNode<AimToNearEnemy>("AimToNearEnemy").SearchRadius = _maxRange;
    }

    private protected override void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.FarthestEnemy!;
        var prj = _projectile.Instantiate<BulletProjectile>();
        {
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = _life;
            prj.ConstantForce = (enemy.GlobalPosition - GlobalPosition).Normalized() * _speed;
            prj.PenetrateEnemy = true;
            prj.PenetrateWall = false;
        }

        AddProjectile(prj, GlobalPosition);

        // ToDo: 暫定実装, 敵にヒットするたびにダメージを半分にする
        prj.Hit
            .Where(x => x.HitNode is EnemyBase)
            .Subscribe(prj, (x, s) => { s.Damage *= 0.5f; })
            .AddTo(prj);
    }
}