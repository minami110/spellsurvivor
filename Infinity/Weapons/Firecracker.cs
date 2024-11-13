using System.Collections.Generic;
using fms.Effect;
using fms.Projectile;
using Godot;
using Godot.Collections;

namespace fms.Weapon;

/// <summary>
/// 爆竹を生成する
/// </summary>
public partial class Firecracker : WeaponBase
{
    [ExportGroup("Bomb Projectile")]
    [Export]
    private PackedScene _projectile0 = null!;

    /// <summary>
    /// 爆弾の投擲速度
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _throwSpeed = 100f;

    [Export(PropertyHint.Range, "0,9999,1,suffix:frames")]
    private uint _bombLife = 120u;

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 200f;

    [ExportGroup("Area Projectile")]
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _areaRadius = 30u;

    [Export(PropertyHint.Range, "0,9999,1,suffix:frames")]
    private uint _areaDamageSpan = 10u;

    [Export(PropertyHint.Range, "0,9999,1,suffix:frames")]
    private uint _areaLife = 90u;

    [ExportGroup("For Debugging")]
    [Export]
    private int TrickShotCount { get; set; }

    [Export]
    private float TrickShotDamageMul { get; set; }

    public override void _Ready()
    {
        GetNode<AimToNearEnemy>("AimToNearEnemy").SearchRadius = _maxRange;
    }

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
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var target = aim.NearestEnemy!;

        // 爆弾本体
        var bomb = _projectile0.Instantiate<BaseProjectile>();
        {
            bomb.Damage = 0f; // 本体はダメージを与えない
            bomb.Knockback = 0u; // 本体はノックバックを与えない
            bomb.LifeFrame = _bombLife;
            bomb.ConstantForce = (target.GlobalPosition - GlobalPosition).Normalized() * _throwSpeed;

            bomb.AddChild(new DeathTrigger
            {
                Next = SpawnAreaDamage,
                When = WhyDead.CollidedWithAny | WhyDead.Life // 何かにぶつかった or 寿命が来たらエリアダメージを展開する
            });
        }

        AddProjectile(bomb, GlobalPosition);
    }

    /// <summary>
    /// 爆発後のエリアダメージを生成する処理, DeathTrigger のコールバックで呼び出される
    /// </summary>
    /// <returns></returns>
    private BaseProjectile SpawnAreaDamage(Dictionary info)
    {
        var area = new CircleAreaProjectile();
        area.Damage = BaseDamage;
        area.Knockback = Knockback;
        area.DamageEveryXFrames = _areaDamageSpan;
        area.LifeFrame = _areaLife;
        area.Radius = _areaRadius;
        area.Offset = new Vector2(0, 0);
        area.Position = (Vector2)info["DeadPosition"];

        return area;
    }
}