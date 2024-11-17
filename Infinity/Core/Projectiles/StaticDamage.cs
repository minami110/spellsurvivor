﻿using System;
using System.Collections.Generic;
using System.Linq;
using fms.Weapon;
using Godot;
using R3;

namespace fms.Projectile;

public partial class StaticDamage : Area2D
{
    private readonly List<ExcludeInfo> _excludes = [];

    /// <summary>
    /// Projectile のダメージ
    /// </summary>
    internal float Damage { get; set; }

    /// <summary>
    /// ノックバック速度
    /// </summary>
    internal uint Knockback { get; set; }

    protected CollisionShape2D CollisionShape
    {
        get
        {
            // Find CollisionShape2D
            var collisionShape = this.FindFirstChild<CollisionShape2D>();

            // Do not exist, create new one
            if (collisionShape is null)
            {
                collisionShape = new CollisionShape2D();
                AddChild(collisionShape);
            }

            return collisionShape;
        }
    }

    public bool Disabled
    {
        get => CollisionShape.Disabled;
        set => CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, value);
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationEnterTree:
            {
                // Note: ウェーブ終了時に死んでほしくないのでグループに追加しない
                // AddToGroup(Constant.GroupNameProjectile);

                this.BodyEnteredAsObservable()
                    .Subscribe(this, (x, s) => s.OnBodyEntered(x))
                    .AddTo(this);

                // Note: Override しないと動かないので手動で
                SetPhysicsProcess(true);
                break;
            }
            case NotificationPhysicsProcess:
            {
                UpdateExcludes();
                break;
            }
        }
    }

    private protected virtual void OnBodyEntered(Node2D body)
    {
        var weapon = FindWeaponInParent();

        // 敵との衝突時の処理
        if (body is IEntity entity)
        {
            // 除外リストに含まれている場合は無視
            if (_excludes.Any(x => x.Node == body))
            {
                return;
            }

            // ToDo: IEntity に雑にキャスト
            entity.ApplayDamage(Damage, (IEntity)weapon.OwnedEntity, weapon);
            SendHitInfo(body);

            // ToDo: Knockback 処理, 型があいまい
            // Note: 死んでても死亡時アニメーションがあるのでノックバックを与える
            if (body is EnemyBase enemy)
            {
                if (Knockback > 0)
                {
                    var dir = (enemy.GlobalPosition - GlobalPosition).Normalized();
                    var impulse = dir * Knockback;
                    enemy.ApplyKnockback(impulse);
                }
            }

            // 敵を貫通する場合 多段ヒットを防止するために除外リストに追加 
            _excludes.Add(new ExcludeInfo
            {
                Node = body,
                Created = Engine.GetPhysicsFrames()
            });
        }
    }

    /// <summary>
    /// Projectile を生成した Weapon を取得する, 先祖の何処かにいるはずなので再帰的に検索する
    /// </summary>
    /// <returns></returns>
    private WeaponBase FindWeaponInParent()
    {
        var parent = GetParent();

        while (parent is not null)
        {
            if (parent is WeaponBase weapon)
            {
                return weapon;
            }

            parent = parent.GetParent();
            parent.GetParentOrNull<WeaponBase>();
        }

        throw new InvalidProgramException("Failed to find WeaponBase");
    }

    private void SendHitInfo(Node2D node)
    {
        /*
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
        */
    }

    private void UpdateExcludes()
    {
        if (_excludes.Count == 0)
        {
            return;
        }

        var current = Engine.GetPhysicsFrames();

        // 5 Frame 後には除外リストから削除
        // リストを逆順にして削除していく
        for (var i = _excludes.Count - 1; i >= 0; i--)
        {
            var exclude = _excludes[i];
            if (current - exclude.Created <= 5UL)
            {
                continue;
            }

            _excludes.RemoveAt(i);
        }
    }

    private struct ExcludeInfo
    {
        public Node2D Node { get; init; }
        public ulong Created { get; init; }
    }
}