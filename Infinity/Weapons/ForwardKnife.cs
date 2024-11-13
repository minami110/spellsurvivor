using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;
using Godot.Collections;

namespace fms.Weapon;

public partial class ForwardKnife : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 500f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120u;

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

    private static void RegisterTrickshot(BaseProjectile parent, Dictionary payload)
    {
        parent.AddChild(new TrickshotMod
        {
            Next = SpawnTrickshotProjectile,
            When = WhyDead.CollidedWithAny,
            SearchRadius = 200, // Trickshot 実行時の敵検索範囲,
            Payload = payload
        });
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        // Main の Projectile
        var prjMain = _projectile.Instantiate<BaseProjectile>();
        {
            prjMain.Damage = BaseDamage;
            prjMain.Knockback = Knockback;
            prjMain.LifeFrame = _life;

            // Get Player's aim direction
            var direction = GetParent<BasePlayerPawn>().LatestMoveDirection;
            prjMain.ConstantForce = direction * _speed;
        }

        // Trickshot が有効な場合は登録する
        if (TrickShotCount > 0 || TrickShotDamageMul > 0f)
        {
            var payload = new Dictionary
            {
                { "BaseDamage", BaseDamage },
                { "Knockback", Knockback },
                { "Speed", _speed },
                { "Life", _life },
                { "TrickShotCount", TrickShotCount },
                { "TrickShotDamageMul", TrickShotDamageMul },
                { "ProjectileScene", _projectile }
            };

            RegisterTrickshot(prjMain, payload);
        }

        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
        AddProjectile(prjMain, spawnPos);
    }

    private static BaseProjectile SpawnTrickshotProjectile(Dictionary payload)
    {
        var prj = ((PackedScene)payload["ProjectileScene"]).Instantiate<BaseProjectile>();

        prj.Position = (Vector2)payload["DeadPosition"];
        prj.Damage = (float)payload["BaseDamage"] * (float)payload["TrickShotDamageMul"];
        prj.Knockback = (uint)payload["Knockback"];
        prj.LifeFrame = (uint)payload["Life"];
        prj.ConstantForce = (Vector2)payload["Direction"] * (float)payload["Speed"];

        if ((int)payload["Iter"] < (int)payload["TrickShotCount"])
        {
            RegisterTrickshot(prj, payload);
        }

        return prj;
    }
}