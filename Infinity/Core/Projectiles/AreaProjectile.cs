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

    public override void _Process(double delta)
    {
        if (_attackLock)
        {
            return;
        }

        // ダメージ処理を実行
        if (Age >= FirstSleepFrames)
        {
            if(DamageEveryXFrames == 0)
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

    private protected override void OnBodyEntered(Node2D body)
    {
        // 親の衝突時のダメージ処理もろもろの処理を無効化する
        return;
    }

    /// <summary>
    /// ダメージを与える処理
    /// </summary>
    private protected virtual void Attack()
    {
        var bodies = GetOverlappingBodies();

        if (bodies.Count > 0)
        {
            foreach (var body in bodies)
            {
                if (body is Enemy enemy)
                {
                    enemy.TakeDamage(Damage, Weapon);
                }
            }
        }
    }
}