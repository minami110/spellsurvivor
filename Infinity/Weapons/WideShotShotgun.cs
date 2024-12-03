using fms.Projectile;
using Godot;
using R3;

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

    private AimToNearEnemy AimToNearEnemy => GetNode<AimToNearEnemy>("AimToNearEnemy");

    public override void _Ready()
    {
        AimToNearEnemy.SearchRadius = _maxRange;
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        if (AimToNearEnemy.IsAiming)
        {
            SpawnProjectile();
        }
        else
        {
            AimToNearEnemy.EnteredAnyEnemy
                .Take(1)
                .SubscribeAwait(async (_, _) =>
                {
                    // AimToNearEnemy が対象を狙うまでちょっと待つ必要がある
                    await this.WaitForSecondsAsync(0.1f);
                    SpawnProjectile();
                })
                .AddTo(this);
        }
    }

    private void SpawnProjectile()
    {
        var mazzle = GetNode<Node2D>("AimToNearEnemy/MazzlePoint");

        // プレイヤーの正面から spredAngle の範囲で均等に numberOfProjectiles の数だけ弾を発射する
        for (var i = 0; i < _numberOfProjectiles; i++)
        {
            var prj = _projectile.Instantiate<BaseProjectile>();
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.LifeFrame = _life;

            var angle = _spreadAngle * (i - _numberOfProjectiles / 2f) / _numberOfProjectiles;
            var dir = mazzle.GlobalTransform.X.Rotated(Mathf.DegToRad(angle));
            prj.ConstantForce = dir * _speed;

            AddProjectile(prj, mazzle.GlobalPosition);
        }

        RestartCoolDown();
    }
}