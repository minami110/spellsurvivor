using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using R3;

namespace fms.Projectile;

public partial class BulletProjectile : BaseProjectile
{
    [Flags]
    public enum PenetrateType
    {
        None = 0,
        Enemy = 1 << 0,
        Wall = 1 << 1
    }

    private readonly List<ExcludeInfo> _excludes = [];

    public PenetrateType PenetrateSettings { get; set; }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationReady)
        {
            this.BodyEnteredAsObservable()
                .Subscribe(this, (x, s) => s.OnBodyEntered(x))
                .AddTo(this);
        }
        else if (what == NotificationPhysicsProcess)
        {
            UpdateExcludes();
        }
    }

    private protected virtual void OnBodyEntered(Node2D body)
    {
        if (Age < SLEEP_FRAME)
        {
            return;
        }

        // 壁など静的なオブジェクトとの衝突時の処理
        if (body is StaticBody2D staticBody)
        {
            if (!PenetrateSettings.HasFlag(PenetrateType.Wall))
            {
                Kill(WhyDead.CollidedWithWall);
            }
        }

        // 敵との衝突時の処理
        else if (body is IEntity entity)
        {
            // 除外リストに含まれている場合は無視
            if (_excludes.Any(x => x.Node == body))
            {
                return;
            }

            ApplayDamageToEntity(entity, Damage);
            SendHitInfo(body);

            // ToDo: Knockback 処理, 型があいまい
            // Note: 死んでても死亡時アニメーションがあるのでノックバックを与える
            if (body is EntityEnemy enemy)
            {
                if (Knockback > 0)
                {
                    var impulse = PrevLinearVelocity.Normalized() * Knockback;
                    enemy.ApplyKnockback(impulse);
                }
            }

            // 敵を貫通しないなら
            if (!PenetrateSettings.HasFlag(PenetrateType.Enemy))
            {
                Kill(WhyDead.CollidedWithEnemy);
                return;
            }

            // 敵を貫通する場合 多段ヒットを防止するために除外リストに追加 
            _excludes.Add(new ExcludeInfo
            {
                Node = body,
                CreatedAge = Age
            });
        }
    }

    private void SendHitInfo(Node2D node)
    {
        // 最新の HitInfo を更新 (ダメージ処理のあとにやること)
        // Note: すべての当たり判定が Sphere という決め打ちで法線を計算しています
        HitInfo = new ProjectileHitInfo
        {
            HitNode = node,
            Position = GlobalPosition,
            Normal = (GlobalPosition - node.GlobalPosition).Normalized(),
            Velocity = PrevLinearVelocity
        };

        // Hit を通知する
        _hitSubject.OnNext(HitInfo);
    }

    private void UpdateExcludes()
    {
        // 5 Frame 後には除外リストから削除
        // Note: 5 Frame は適当な値っす
        // リストを逆順にして削除していく
        for (var i = _excludes.Count - 1; i >= 0; i--)
        {
            var exclude = _excludes[i];
            if (Age - exclude.CreatedAge <= 5u)
            {
                continue;
            }

            _excludes.RemoveAt(i);
        }
    }

    private struct ExcludeInfo
    {
        public Node2D Node { get; init; }
        public uint CreatedAge { get; init; }
    }
}