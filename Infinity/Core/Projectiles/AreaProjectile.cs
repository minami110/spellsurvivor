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
            if (IsDead ||_attackLock || Age < _SLEEP_FRAME)
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
        // そのため, 最初の敵に対してのみ HitInfo を生成する
        var isHitInfoGenerated = false;

        // 範囲内すべての敵にダメージを与える
        foreach (var body in bodies)
        {
            if (body is Enemy enemy)
            {
                enemy.TakeDamage(Damage, Weapon);

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