using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
///     爆竹を生成する
/// </summary>
public partial class FirecrackerGen : WeaponBase
{
    [Export]
    private PackedScene _projectile0 = null!;

    [Export]
    private PackedScene _projectile1 = null!;

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
        var prj0 = ConstructProj(false);

        if (TrickShotCount >= 1)
        {
            // Trickshot 1
            var prj1 = ConstructProj(true);
            prj0.AddChild(new DeathTrigger
            {
                Next = prj1,
                When = WhyDead.CollidedWithAny
            });
            
            if (TrickShotCount >= 2)
            {
                // Trickshot 2
                var prj2 = ConstructProj(true);
                prj1.AddChild(new DeathTrigger
                {
                    Next = prj2,
                    When = WhyDead.CollidedWithAny
                });
            }
        }
        

        FrameTimer.AddChild(prj0);
    }

    private BaseProjectile ConstructProj(bool trickshot)
    {
        // 根本のダメージない奴
        var mainPrj = _projectile0.Instantiate<BaseProjectile>();
        if (!trickshot)
        {
            mainPrj.GlobalPosition = GlobalPosition;
        }

        // 自動で敵狙う, 敵いないなら打たない
        mainPrj.AddChild(new AutoAim
        {
            Mode = AutoAimMode.KillPrjWhenSearchFailed,
            SearchRadius = 200
        });

        // 毒沼部分
        var subPrj = _projectile1.Instantiate<BaseProjectile>();

        if (trickshot)
        {
            subPrj.AddChild(new DamageMod { Multiply = TrickShotDamageMul });
        }

        mainPrj.AddChild(new DeathTrigger
        {
            Next = subPrj,
            When = WhyDead.CollidedWithAny
        });

        return mainPrj;
    }
}