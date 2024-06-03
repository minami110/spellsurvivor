using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class ForwardKnife : WeaponBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    private int _trickshotBounceCount;
    private float _trickshotBounceDamageMultiplier;

    private protected override int BaseCoolDownFrame => 60;

    private protected override void DoAttack()
    {
        switch (MinionLevel)
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

    private protected override void OnSolveEffect(IReadOnlyList<EffectBase> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
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
        var bullet = _bulletPackedScene.Instantiate<TrickshotArrow>();
        {
            bullet.BaseSpeed = 1000f;
            bullet.BaseDamage = 50;
            bullet.InitialVelocity = GlobalTransform.X; // Forward
            bullet.InitialPosition = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
            bullet.BounceCount = _trickshotBounceCount;
            bullet.BounceDamageMultiplier = _trickshotBounceDamageMultiplier;
            bullet.BounceSearchRadius = 400f;
        }

        _bulletSpawnNode.AddChild(bullet);
    }
}