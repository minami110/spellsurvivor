using System;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class Claw : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 60f;

    private uint _enemyHitReduceCounter;
    private uint _enemyHitStacks;

    public override void _Ready()
    {
        GetNode<AimToNearEnemy>("AimToNearEnemy").SearchRadius = _maxRange;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_enemyHitStacks > 0)
        {
            // スタックの最大数は 5 にする
            _enemyHitStacks = Math.Min(_enemyHitStacks, 5);

            // カウンタをすすめてスタックの数を 1 減らす
            // Note: スタックが貯まるたびに猶予時間が短くなるイメージの式
            _enemyHitReduceCounter++;
            if (_enemyHitStacks > 4)
            {
                if (_enemyHitReduceCounter >= 30)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
            else if (_enemyHitStacks > 2)
            {
                if (_enemyHitReduceCounter >= 40)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
            else
            {
                if (_enemyHitReduceCounter >= 60)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
        }
        else
        {
            _enemyHitReduceCounter = 0;
        }

        // スタック数に応じてクールダウンを短く設定する
        // 0: 60f, 1: 50f, 2: 40f, 3: 30f, 4: 20f, 5: 10f
        var newCoolDown = 60u - _enemyHitStacks * 10u;
        BaseCoolDownFrame = newCoolDown;
    }

    private protected override void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.NearestEnemy;

        // 弾生成
        var prj = _projectile.Instantiate<AreaProjectile>();
        {
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = 30u;
        }

        // 敵の方向を向くような rotation を計算する
        var dir = enemy!.GlobalPosition - GlobalPosition;
        var angle = dir.Angle();

        // 自分の位置から angle 方向に 15 伸ばした位置を計算する
        // Note: プレイ間確かめながらスポーン位置のピクセル数は調整する
        var pos = GlobalPosition + dir.Normalized() * 15;

        AddProjectile(prj, pos, angle);

        // Note: AddTo はシーンに入れたあとしかできないので
        // Projectile が Enemy にヒットしたら Stack を一つ貯める
        // Prj は貫通するが, 一つの Prj につき 1回だけ Stack を貯められるので Take(1) を入れている
        prj.Hit.Where(x => x.HitNode is EnemyBase)
            .Take(1)
            .Subscribe(hitInfo =>
            {
                _enemyHitStacks++;
                _enemyHitReduceCounter = 0;
            })
            .AddTo(prj);
    }
}