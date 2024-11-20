using System;
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
    // 反射回数
    [Export]
    private int BounceCount { get; set; }

    [Export]
    // 反射時のダメージ倍率
    private float BounceDamageMul { get; set; }

    private protected override void OnCoolDownCompleted(uint level)
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

        RestartCoolDown();
    }

    private protected override void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
        BounceCount = 0;
        BounceDamageMul = 0f;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                // この武器は Trickshot に対応しているので拾う
                case TrickshotBounce trickshotBounceCount:
                {
                    BounceCount += trickshotBounceCount.BounceCount;
                    BounceDamageMul += trickshotBounceCount.BounceDamageMultiplier;
                    break;
                }
            }
        }
    }

    // 引数の Projectile に Trickshot Mod を登録する
    private static void AddTrickshotMod(BaseProjectile parent, Dictionary payload)
    {
        parent.AddChild(new TrickshotMod
        {
            Next = SpawnNextProjectile,
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
            if (OwnedEntity is IPawn pawn)
            {
                prjMain.ConstantForce = pawn.GetAimDirection() * _speed;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // Trickshot が有効な場合は Mod を登録する
        if (BounceCount > 0 || BounceDamageMul > 0f)
        {
            var payload = new Dictionary
            {
                { "BaseDamage", BaseDamage },
                { "Knockback", Knockback },
                { "Speed", _speed },
                { "Life", _life },
                { "TrickShotCount", BounceCount },
                { "TrickShotDamageMul", BounceDamageMul },
                { "ProjectileScene", _projectile }
            };

            AddTrickshotMod(prjMain, payload);
        }

        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
        AddProjectile(prjMain, spawnPos);
    }

    // ヒット時に Mod から呼ばれるコールバック, 次の弾を生成する 
    private static BaseProjectile SpawnNextProjectile(Dictionary payload)
    {
        var prj = ((PackedScene)payload["ProjectileScene"]).Instantiate<BaseProjectile>();

        prj.Position = (Vector2)payload["DeadPosition"];
        prj.Damage = (float)payload["BaseDamage"] * (float)payload["TrickShotDamageMul"];
        prj.Knockback = (uint)payload["Knockback"];
        prj.LifeFrame = (uint)payload["Life"];
        prj.ConstantForce = (Vector2)payload["Direction"] * (float)payload["Speed"];

        if ((int)payload["Iter"] < (int)payload["TrickShotCount"])
        {
            AddTrickshotMod(prj, payload);
        }

        return prj;
    }
}