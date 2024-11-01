using Godot;
using R3;

namespace fms.Projectile;

public partial class BulletProjectile : BaseProjectile
{
    [Export]
    private protected bool PenetrateEnemy {get; private set;}

    [Export]
    private protected bool PenetrateWall {get; private set;}

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

        // 最新の HitInfo を更新
        // Note: すべての当たり判定が Sphere という決め打ちで法線を計算しています
        HitInfo = new ProjectileHitInfo
        {
            HitNode = body,
            Position = GlobalPosition,
            Normal = (GlobalPosition - body.GlobalPosition).Normalized(),
            Velocity = Direction.Normalized() * Speed
        };

        _hitSubject.OnNext(HitInfo);

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
        else if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage, Weapon);

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
    }
}