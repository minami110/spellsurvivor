using System;
using Godot;

namespace fms.Projectile;

/// <summary>
/// 角度に制限がある(円弧形状の) CircleAreaProjectile
/// </summary>
public partial class ArcAreaProjectile : CircleAreaProjectile
{
    private const float _ANGLE_LIMIT_MIN = 0f;
    private const float _ANGLE_LIMIT_MAX = 360f;

    // 独自に当たり判定を描画するので Godot のほうは透明に
    private protected override Color DebugColor { get; set; } = new("00000000");

    public float AngleLimit
    {
        get;
        set
        {
            var newvalue = Mathf.Clamp(value, _ANGLE_LIMIT_MIN, _ANGLE_LIMIT_MAX);
            if (Math.Abs(field - newvalue) <= 0.0001f)
            {
                return;
            }

            field = newvalue;
        }
    } = 90f;

    // Note: エフェクト用意するのがめんどいので, Godot の Draw 関数を使って当たり判定を描写してます
    public override void _Draw()
    {
        // 現在の設定を見に行く
        var config = GameConfig.Singleton;
        if (!config.DebugShowCollision.CurrentValue)
        {
            return;
        }

        var center = Offset;
        var radius = Radius;
        var angleRad = Mathf.DegToRad(AngleLimit);

        // 円弧の端の線, 中心の線 3本を描画
        // 一番左の線のゴール
        var left = new Vector2(
            center.X + radius * Mathf.Cos(angleRad / 2),
            center.Y + radius * Mathf.Sin(angleRad / 2)
        );
        DrawLine(center, left, new Color(0, 1, 0));

        // 右の線
        var right = new Vector2(
            center.X + radius * Mathf.Cos(-angleRad / 2),
            center.Y + radius * Mathf.Sin(-angleRad / 2)
        );
        DrawLine(center, right, new Color(0, 1, 0));

        // 左と右の端 を円周上につなぐ線を 5分割して描画する
        var div = 5;
        for (var i = 0; i < div; i++)
        {
            var start = new Vector2(
                center.X + radius * Mathf.Cos(-angleRad / 2 + angleRad / div * i),
                center.Y + radius * Mathf.Sin(-angleRad / 2 + angleRad / div * i)
            );
            var end = new Vector2(
                center.X + radius * Mathf.Cos(-angleRad / 2 + angleRad / div * (i + 1)),
                center.Y + radius * Mathf.Sin(-angleRad / 2 + angleRad / div * (i + 1))
            );
            DrawLine(start, end, new Color(0, 1, 0));
        }
    }

    // ダメージを与える処理 をオーバーライド
    private protected override void Attack()
    {
        // 角度制限が 0 以下の場合は攻撃しないと等しい
        if (AngleLimit <= 0f)
        {
            return;
        }

        // 角度制限が 180 以上の場合は通常の円形範囲攻撃と等しい
        if (AngleLimit >= 180f)
        {
            base.Attack();
            return;
        }

        // 以下は角度制限が 0 より大きく 180 より小さい場合の処理
        // 基本親の実装と同じだけど, ループの先頭で角度の判定を行う
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
        {
            return;
        }

        // Hit Info を生成する
        // ToDo: 複数の敵にダメージを与えたため, 法線などが正しく計算できない
        var isHitInfoGenerated = false;

        // 範囲内すべての Entity にダメージを与える, Entity のフィルタはレイヤー設定で行う
        foreach (var body in bodies)
        {
            // 自分の正面と敵へのベクトルの角度を計算
            var angleRad = (body.GlobalPosition - GlobalPosition).Angle();
            var diffRad = Mathf.Abs(GlobalRotation - angleRad);
            if (diffRad > Mathf.Pi)
            {
                diffRad = Mathf.Tau - diffRad;
            }

            // 角度制限内に収まっていない場合は攻撃しない
            if (diffRad > Mathf.DegToRad(AngleLimit))
            {
                continue;
            }

            if (body is IEntity entity)
            {
                ApplayDamageToEntity(entity, Damage);

                // 複数の敵にヒットするため, HitInfo は最初の敵に対してのみ生成する
                // Note: Hit 通知に依存した武器の Stack 処理などがあるため, Hit したかどうかは 1回の攻撃で1回まで
                //       という仕様にしています
                if (!isHitInfoGenerated)
                {
                    isHitInfoGenerated = true;
                    HitInfo = new ProjectileHitInfo
                    {
                        HitNode = body,
                        Position = GlobalPosition,
                        Normal = (GlobalPosition - body.GlobalPosition).Normalized(),
                        Velocity = PrevLinearVelocity
                    };

                    _hitSubject.OnNext(HitInfo);
                }

                // ToDo: Knockback 処理, 型があいまい
                // Note: 死んでても死亡時アニメーションがあるのでノックバックを与える
                if (body is EntityEnemy enemy)
                {
                    if (Knockback > 0)
                    {
                        // ToDo: 暫定的に Area Projectile は 自身の位置から敵の位置に向かうノックバックを与えてます
                        var vel = body.GlobalPosition - GlobalPosition;
                        var impulse = vel.Normalized() * Knockback;
                        enemy.ApplyKnockback(impulse);
                    }
                }
            }
        }
    }
}