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
    private PackedScene _projectile = null!;

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
        var bomb = _projectile.Instantiate<BaseProjectile>();
        {
            bomb.Damage = 0f; // 本体はダメージを与えない
            bomb.Knockback = 0u; // 本体はノックバックを与えない
            bomb.LifeFrame = _bombLife;
            bomb.ConstantForce = (target.GlobalPosition - GlobalPosition).Normalized() * _throwSpeed;
        }

        // Mod に渡すパラメータを設置する
        var payload = new Dictionary
        {
            // Bomb
            { "ProjectileScene", _projectile },
            { "ThrowSpeed", _throwSpeed },
            { "BombLife", _bombLife },
            // Area
            { "Knockback", Knockback },
            { "BaseDamage", BaseDamage },
            { "AreaRadius", _areaRadius },
            { "AreaDamageSpan", _areaDamageSpan },
            { "AreaLife", _areaLife },
            // Trickshot 用
            { "TrickShotCount", TrickShotCount },
            { "TrickShotDamageMul", TrickShotDamageMul }
        };

        // 爆発後のエリアダメージを生成する処理を登録
        bomb.AddChild(new DeathTrigger
        {
            Next = SpawnAreaDamage,
            When = WhyDead.CollidedWithAny | WhyDead.Life, // 何かにぶつかった or 寿命が来たらエリアダメージを展開する
            Payload = payload
        });

        // Trickshot が有効な場合は登録する
        if (TrickShotCount > 0 || TrickShotDamageMul > 0f)
        {
            RegisterTrickshot(bomb, payload);
        }

        AddProjectile(bomb, GlobalPosition);
    }

    private static void RegisterTrickshot(BaseProjectile parent, Dictionary payload)
    {
        parent.AddChild(new TrickshotMod
        {
            Next = SpawnTrickshotProjectile,
            When = WhyDead.CollidedWithAny, // 敵にぶつかったときだけ Trickshot を発動する
            SearchRadius = 200, // Trickshot 実行時の敵検索範囲,
            Payload = payload
        });
    }

    private static BaseProjectile SpawnAreaDamage(Dictionary payload)
    {
        var area = new CircleAreaProjectile();

        area.Damage = (float)payload["BaseDamage"];
        // Trickshot 中での呼び出しであればダメージを調整する
        if (payload.ContainsKey("Iter"))
        {
            area.Damage *= (float)payload["TrickShotDamageMul"];
        }

        area.Knockback = (uint)payload["Knockback"];
        area.DamageEveryXFrames = (uint)payload["AreaDamageSpan"];
        area.LifeFrame = (uint)payload["AreaLife"];
        area.Radius = (float)payload["AreaRadius"];
        area.Offset = new Vector2(0, 0);

        // 爆弾が消滅した位置にスポーンする
        area.Position = (Vector2)payload["DeadPosition"];

        return area;
    }

    private static BaseProjectile SpawnTrickshotProjectile(Dictionary payload)
    {
        var prj = ((PackedScene)payload["ProjectileScene"]).Instantiate<BaseProjectile>();

        prj.Position = (Vector2)payload["DeadPosition"];
        prj.Damage = 0u;
        prj.Knockback = 0u;
        prj.LifeFrame = (uint)payload["BombLife"];
        prj.ConstantForce = (Vector2)payload["Direction"] * (float)payload["ThrowSpeed"];

        // 爆発後のエリアダメージを生成する処理を登録
        prj.AddChild(new DeathTrigger
        {
            Next = SpawnAreaDamage,
            When = WhyDead.CollidedWithAny | WhyDead.Life, // 何かにぶつかった or 寿命が来たらエリアダメージを展開する
            Payload = payload
        });

        // Trickshot の残数が残っている場合はまた登録する
        if ((int)payload["Iter"] < (int)payload["TrickShotCount"])
        {
            RegisterTrickshot(prj, payload);
        }

        return prj;
    }
}