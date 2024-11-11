using System;
using fms.Weapon;
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
    /// Projectile のダメージ
    /// </summary>
    [Export]
    public float Damage { get; set; }

    /// <summary>
    /// Projectile の寿命 (フレーム数)
    /// </summary>
    [Export]
    public uint LifeFrame { get; set; } = _FORCED_LIFETIME;

    /// <summary>
    /// Projectile の 1秒あたりの速度 (px)
    /// </summary>
    [Export]
    public uint Speed { get; set; }

    /// <summary>
    /// ノックバック速度
    /// </summary>
    [Export]
    public uint Knockback { get; set; }

    private const uint _FORCED_LIFETIME = 7200u;

    /// <summary>
    /// 生成直後ダメージを与えない猶予期間, この期間はチラツキ防止で描写も行われない
    /// Note: 1F 目は Position の解決, 2F 目は Mod による位置の解決 でなんか丁度いい値
    /// const ではなくてメンバにしたほうが柔軟かも
    /// </summary>
    private protected const uint _SLEEP_FRAME = 2u;

    private readonly Subject<WhyDead> _deadSubject = new();

    private protected readonly Subject<ProjectileHitInfo> _hitSubject = new();

    public ProjectileHitInfo HitInfo { get; internal set; }

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
    /// 現在進む方向
    /// </summary>
    public Vector2 Direction { get; set; }

    /// <summary>
    /// この Projectile を発射した Weapon (OnReady で自動で取得)
    /// </summary>
    public WeaponBase Weapon { get; private set; } = null!;

    public bool IsDead => _deadSubject.IsDisposed;

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
                // Note: スポーン, 位置補正 Mod などのチラツキ防止もあり, Sleep 中は非表示にしておく
                if (_SLEEP_FRAME > 0)
                {
                    Hide();
                }

                _deadSubject.AddTo(this);
                _hitSubject.AddTo(this);
                SetProcess(true);

                try
                {
                    Weapon = GetWeaponInParent();
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidProgramException("Projectile の2つ親が WeaponBase ではありません", e);
                }

                break;
            }
            case NotificationProcess:
            {
                // 寿命を増加させる
                Age++;

                if (Age > _SLEEP_FRAME)
                {
                    Show();
                }

                // 移動処理を行う
                if (Speed > 0 && Direction.LengthSquared() > 0)
                {
                    var deltaTime = GetProcessDeltaTime();
                    var velocity = Direction.Normalized() * Speed;
                    GlobalPosition += velocity * (float)deltaTime;
                    GlobalRotation = velocity.Angle();
                }

                // 寿命が 0 の場合は無限に生存する
                if (LifeFrame > 0)
                {
                    // 寿命が来たら消滅
                    if (Age > LifeFrame)
                    {
                        OnDead(WhyDead.Life);
                    }
                }

                break;
            }
        }
    }

    public virtual void Kill(WhyDead reason)
    {
        OnDead(reason);
    }

    /// <summary>
    /// Projectile を生成した Weapon を取得する, 先祖の何処かにいるはずなので再帰的に検索する
    /// </summary>
    /// <returns></returns>
    private WeaponBase GetWeaponInParent()
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