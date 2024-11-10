using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class ForwardKnife : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [ExportGroup("For Debugging")]
    [Export]
    private int TrickShotCount { get; set; }

    [Export]
    private float TrickShotDamageMul { get; set; }

    private protected override void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
        TrickShotCount = 0;
        TrickShotDamageMul = 0f;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                // この武器は Trickshot に対応しているので拾う
                case TrickshotBounce trickshotBounceCount:
                {
                    TrickShotCount += trickshotBounceCount.BounceCount;
                    TrickShotDamageMul += trickshotBounceCount.BounceDamageMultiplier;
                    break;
                }
            }
        }
    }

    private protected override void SpawnProjectile(uint level)
    {
        switch (level)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet(GlobalPosition);
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(GlobalPosition, 10f);
                SpawnBullet(GlobalPosition, -10f);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(GlobalPosition, 20f);
                SpawnBullet(GlobalPosition, 0f, 10f);
                SpawnBullet(GlobalPosition, -20f);
                break;
            }
        }
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        // Main の Projectile
        var prj1 = _projectile.Instantiate<BaseProjectile>();
        {
            prj1.Damage = BaseDamage;
            prj1.Knockback = Knockback;
            prj1.LifeFrame = 120u;
            prj1.Speed = 500u;
        }

        if (TrickShotCount >= 1)
        {
            var prj2 = _projectile.Instantiate<BaseProjectile>();
            {
                prj2.Damage = BaseDamage;
                prj2.Knockback = Knockback;
                prj2.LifeFrame = 120u;
                prj2.Speed = 500u;
            }

            prj2.AddChild(new DamageMod { Multiply = TrickShotDamageMul });
            prj2.AddChild(new AutoAim
            {
                Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
                SearchRadius = 100
            });
            prj1.AddChild(new DeathTrigger { Next = prj2, When = WhyDead.CollidedWithAny });

            if (TrickShotCount >= 2)
            {
                var prj3 = _projectile.Instantiate<BaseProjectile>();
                {
                    prj3.Damage = BaseDamage;
                    prj3.Knockback = Knockback;
                    prj3.LifeFrame = 120u;
                    prj3.Speed = 500u;
                }

                prj3.AddChild(new DamageMod { Multiply = TrickShotDamageMul });
                prj3.AddChild(new AutoAim
                {
                    Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
                    SearchRadius = 100
                });
                prj2.AddChild(new DeathTrigger { Next = prj3, When = WhyDead.CollidedWithAny });
            }
        }

        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;

        // Get Player's aim direction
        var direction = GetParent<BasePlayerPawn>().LatestMoveDirection;
        AddProjectile(prj1, spawnPos, direction);
    }
}