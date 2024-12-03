using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class AssaultRifle : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 200f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120u;

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 100f;

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
        var prj = _projectile.Instantiate<BulletProjectile>();
        {
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.LifeFrame = _life;
            prj.ConstantForce = AimToNearEnemy.GlobalTransform.X * _speed;
            prj.PenetrateEnemy = false;
            prj.PenetrateWall = false;
        }

        AddProjectile(prj, GlobalPosition);
        RestartCoolDown();
    }
}