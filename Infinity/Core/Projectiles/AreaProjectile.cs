using Godot;

namespace fms.Projectile;

public partial class AreaProjectile : BaseProjectile
{
    /// <summary>
    /// 範囲内にいる敵に対して何フレームごとにダメージを与えるか
    /// 0 の場合は一度ダメージを与えて消滅する
    /// </summary>
    [Export]
    public uint DamageEveryXFrames = 5u;

    private bool _attackLock;

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationProcess)
        {
            if (IsDead || _attackLock || Age < _SLEEP_FRAME)
            {
                return;
            }

            if (DamageEveryXFrames == 0)
            {
                _attackLock = true;
                Attack();
            }
            else if (Age % DamageEveryXFrames == 0)
            {
                Attack();
            }
        }
    }

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    private protected virtual void Attack()
    {
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
            if (body is IEntity entity)
            {
                entity.ApplayDamage(Damage, Weapon.GetParent<IEntity>(), Weapon);

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
                        Velocity = Direction.Normalized() * Speed
                    };

                    _hitSubject.OnNext(HitInfo);
                }
            }
        }
    }
}