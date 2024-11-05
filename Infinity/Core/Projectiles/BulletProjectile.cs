using Godot;
using R3;

namespace fms.Projectile;

public partial class BulletProjectile : BaseProjectile
{
    [Export]
    public bool PenetrateEnemy { get; set; }

    [Export]
    public bool PenetrateWall { get; set; }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationReady)
        {
            this.BodyEnteredAsObservable()
                .Subscribe(this, (x, s) => s.OnBodyEntered(x))
                .AddTo(this);
        }
    }

    private protected virtual void OnBodyEntered(Node2D body)
    {
        if (Age < _SLEEP_FRAME)
        {
            return;
        }

        // 壁など静的なオブジェクトとの衝突時の処理
        // デフォルトでは壁と衝突したら常に自身は消滅する
        if (body is StaticBody2D staticBody)
        {
            if (!PenetrateWall)
            {
                Kill(WhyDead.CollidedWithWall);
            }
        }

        // 敵との衝突時の処理
        else if (body is IEntity entity)
        {
            // ToDo: Player / Enemy ともに Weapon が必ず直下に存在している という前提で実装してます
            entity.ApplayDamage(Damage, Weapon.GetParent<IEntity>(), Weapon);

            /*
            if (_hitSound != null)
            {
                SoundManager.PlaySoundEffect(_hitSound);
            }
            */

            if (!PenetrateEnemy)
            {
                Kill(WhyDead.CollidedWithEnemy);
            }
        }

        if (IsDead)
        {
            return;
        }

        // 最新の HitInfo を更新 (ダメージ処理のあとにやること)
        // Note: すべての当たり判定が Sphere という決め打ちで法線を計算しています
        HitInfo = new ProjectileHitInfo
        {
            HitNode = body,
            Position = GlobalPosition,
            Normal = (GlobalPosition - body.GlobalPosition).Normalized(),
            Velocity = Direction.Normalized() * Speed
        };

        _hitSubject.OnNext(HitInfo);
    }
}