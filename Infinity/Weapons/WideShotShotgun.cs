using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class WideShotShotgun : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 500f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120u;

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 200f;

    [Export]
    private uint _numberOfProjectiles = 5;

    [Export]
    private float _spreadAngle = 120f;

    public override void _Ready()
    {
        GetNode<AimToNearEnemy>("AimToNearEnemy").SearchRadius = _maxRange;
    }

    private protected override void OnCoolDownComplete(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        var mazzle = GetNode<Node2D>("AimToNearEnemy/MazzlePoint");

        if (!aim.IsAiming)
        {
            return;
        }

        // プレイヤーの正面から spredAngle の範囲で均等に numberOfProjectiles の数だけ弾を発射する
        for (var i = 0; i < _numberOfProjectiles; i++)
        {
            var prj = _projectile.Instantiate<BaseProjectile>();
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = _life;

            var angle = _spreadAngle * (i - _numberOfProjectiles / 2f) / _numberOfProjectiles;
            var dir = mazzle.GlobalTransform.X.Rotated(Mathf.DegToRad(angle));
            prj.ConstantForce = dir * _speed;

            AddProjectile(prj, mazzle.GlobalPosition);
        }
    }
}