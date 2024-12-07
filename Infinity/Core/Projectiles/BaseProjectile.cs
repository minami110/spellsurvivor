using System;
using Godot;
using R3;

namespace fms.Projectile;

public readonly struct ProjectileHitInfo
{
    public required Node? HitNode { get; init; }
    public required Vector2 Position { get; init; }
    public required Vector2 Normal { get; init; }
    public required Vector2 Velocity { get; init; }
}

public partial class BaseProjectile : Area2D
{
    /// <summary>
    /// Projectile の最大生存時間 (フレーム数)
    /// </summary>
    private const uint _FORCED_LIFETIME = 7200u;

    /// <summary>
    /// 生成直後ダメージを与えない猶予期間, この期間はチラツキ防止で描写も行われない
    /// Note: 1F 目は Position の解決, 2F 目は Mod による位置の解決 でなんか丁度いい値
    /// const ではなくてメンバにしたほうが柔軟かも
    /// </summary>
    private protected const uint SLEEP_FRAME = 2u;

    private readonly Subject<WhyDead> _deadSubject = new();

    private protected readonly Subject<ProjectileHitInfo> _hitSubject = new();

    /// <summary>
    /// 恒常的に与える力
    /// </summary>
    public Vector2 ConstantForce;

    /// <summary>
    /// Projectile のダメージ
    /// </summary>
    internal float Damage { get; set; }

    /// <summary>
    /// Projectile の寿命 (フレーム数)
    /// </summary>
    internal uint LifeFrame
    {
        get;
        set => field = Math.Min(value, _FORCED_LIFETIME);
    } = _FORCED_LIFETIME;

    /// <summary>
    /// ノックバック速度
    /// </summary>
    internal uint Knockback { get; set; }

    public ProjectileHitInfo HitInfo { get; internal set; }

    /// <summary>
    /// Projectile の消滅時に通知
    /// </summary>
    public Observable<WhyDead> Dead => _deadSubject;

    /// <summary>
    /// 敵/壁 を問わずなにかにヒットしたときに通知
    /// ダメージ処理の後に呼ばれる, Projectile 自身が消滅した場合は呼ばれない
    /// </summary>
    public Observable<ProjectileHitInfo> Hit => _hitSubject;

    /// <summary>
    /// 現在の寿命 (フレーム数)
    /// </summary>
    public uint Age { get; private set; }

    /// <summary>
    /// この Projectile を発射した Weapon (OnReady で自動で取得)
    /// </summary>
    public WeaponBase Weapon { get; private set; } = null!;

    /// <summary>
    /// 移動ベクトル
    /// </summary>
    public Vector2 LinearVelocity { get; private set; }

    /// <summary>
    /// 前のフレームの移動ベクトル
    /// </summary>
    public Vector2 PrevLinearVelocity { get; private set; }

    private protected bool IsDead => _deadSubject.IsDisposed;

    internal string CauserPath { get; set; } = string.Empty;

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

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationEnterTree:
            {
                AddToGroup(Constant.GroupNameProjectile);
                break;
            }
            case NotificationReady:
            {
                // Note: スポーン, 位置補正 Mod などのチラツキ防止もあり, Sleep 明けまで非表示にしておく
                Hide();

                _deadSubject.AddTo(this);
                _hitSubject.AddTo(this);

                // Note: Override しないと動かないので手動で
                SetPhysicsProcess(true);

                try
                {
                    Weapon = FindWeaponInParent();
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidProgramException("Projectile の先祖に WeaponBase が見つかりませんでした", e);
                }

                break;
            }
            case NotificationPhysicsProcess:
            {
                // 寿命を増加させる
                Age++;
                if (Age > SLEEP_FRAME)
                {
                    Show();
                }

                // 寿命が 0 の場合は無限に生存する
                if (LifeFrame > 0)
                {
                    // 寿命が来たら消滅
                    if (Age > LifeFrame)
                    {
                        OnDead(WhyDead.Life);
                        return;
                    }
                }

                // 移動処理を行う
                IntegrateForces(GetPhysicsProcessDeltaTime());

                break;
            }
        }
    }

    public void AddLinearVelocity(in Vector2 velocity)
    {
        LinearVelocity += velocity;
    }

    public virtual void Kill(WhyDead reason)
    {
        OnDead(reason);
    }

    private protected virtual void IntegrateForces(double delta)
    {
        LinearVelocity += ConstantForce;

        if (LinearVelocity.LengthSquared() <= 0f)
        {
            PrevLinearVelocity = Vector2.Zero;
            return;
        }

        var motion = LinearVelocity * (float)delta;

        // 位置を更新する
        GlobalPosition += motion;
        GlobalRotation = motion.Angle(); // 移動方向の方を向く

        // LinearVelocity の更新
        PrevLinearVelocity = LinearVelocity;
        LinearVelocity = Vector2.Zero; // Damping: 0 相当の挙動
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

    private void OnDead(WhyDead reason)
    {
        if (IsDead)
        {
            return;
        }

        _deadSubject.OnNext(reason);
        _deadSubject.OnCompleted();
        _hitSubject.OnCompleted();

        // Physics Process から呼ばれる (衝突時死亡) ことがあるので CallDeferred で
        CallDeferred(Node.MethodName.QueueFree);
    }
}