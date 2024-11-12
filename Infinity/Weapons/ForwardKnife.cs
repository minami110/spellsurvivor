using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

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

    // ToDo: Heap Allocation 対策で Static にする
    private void AddTrickShotMod(BaseProjectile parent, PackedScene p, uint currentDepth, uint maxDepth)
    {
        parent.AddChild(new TrickshotMod
        {
            ProjectileSpawnAction = TrickShotSpawnCallback,
            When = WhyDead.CollidedWithAny,
            SearchRadius = 200, // Trickshot 実行時の敵検索範囲,
            Depth = currentDepth,
            MaxDepth = maxDepth,
            Projectile = p
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

        // DeathTrigger を追加する
        if (TrickShotCount > 0 || TrickShotDamageMul > 0f)
        {
            AddTrickShotMod(prjMain, _projectile, 0, (uint)TrickShotCount);
        }

        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
        AddProjectile(prjMain, spawnPos);
    }

    // ToDo: Heap Allocation 対策で Static にする
    private BaseProjectile TrickShotSpawnCallback(PackedScene p, Vector2 pos, Vector2 direction, uint depth,
        uint maxDepth)
    {
        var prj = p.Instantiate<BaseProjectile>();
        prj.Position = pos;
        prj.Damage = BaseDamage * TrickShotDamageMul;
        prj.Knockback = Knockback;
        prj.LifeFrame = _life;
        prj.ConstantForce = direction * _speed;

        if (depth < maxDepth)
        {
            AddTrickShotMod(prj, p, depth, maxDepth);
        }

        return prj;
    }
}