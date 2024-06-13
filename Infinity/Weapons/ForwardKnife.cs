using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class ForwardKnife : WeaponBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _projectile = null!;

    private int _trickshotBounceCount;
    private float _trickshotBounceDamageMultiplier;

    private protected override void DoAttack(uint level)
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

    private protected override void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
        _trickshotBounceCount = 0;
        _trickshotBounceDamageMultiplier = 0f;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                // この武器は Trickshot に対応しているので拾う
                case TrickshotBounce trickshotBounceCount:
                {
                    _trickshotBounceCount += trickshotBounceCount.BounceCount;
                    _trickshotBounceDamageMultiplier += trickshotBounceCount.BounceDamageMultiplier;
                    break;
                }
            }
        }
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
        var block = new SpellBlock(spawnPos, GlobalTransform.X);
        block.Spawn(_projectile).At(new Inherit());
        _ = block.StartAsync(GetNode("FrameTimer"));
    }
}