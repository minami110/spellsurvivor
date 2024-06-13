using Godot;
using R3;

namespace fms.Projectile;

public readonly struct ProjectileHitInfo
{
    public required Node Node { get; init; }
    public required Vector2 Position { get; init; }
    public required Vector2 Normal { get; init; }
    public required Vector2 Velocity { get; init; }
}

public partial class BaseProjectile : Area2D
{
    /// <summary>
    ///     Projectile のダメージ
    /// </summary>
    [Export]
    public float Damage { get; set; }

    /// <summary>
    ///     Projectile の寿命 (フレーム数)
    /// </summary>
    [Export]
    public uint LifeFrame { get; set; } = _FORCED_LIFETIME;

    /// <summary>
    ///     Projectile の 1秒あたりの速度 (px)
    /// </summary>
    [Export]
    public uint Speed { get; set; }

    /// <summary>
    ///     衝突時に消滅するかどうか
    /// </summary>
    [Export]
    public bool OnCollisionDie { get; set; } = true;

    /// <summary>
    ///     継続ダメージの場合何秒に一度攻撃するか
    /// </summary>
    [Export]
    public uint DamageEveryXFrames;

    /// <summary>
    ///     生成直後ダメージを与えない猶予時間 (連続ヒットなどの防止)
    /// </summary>
    [Export]
    public uint FirstSleepFrames = 2;

    [ExportGroup("Sounds")]
    [Export]
    private AudioStream? _hitSound;

    private const uint _FORCED_LIFETIME = 7200;

    private readonly Subject<WhyDead> _deadSubject = new();

    public ProjectileHitInfo HitInfo { get; private set; }

    public Observable<WhyDead> Dead => _deadSubject;

    /// <summary>
    ///     現在の寿命 (フレーム数)
    /// </summary>
    private uint Age { get; set; }

    public Vector2 Direction { get; set; }

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
                if (FirstSleepFrames > 0)
                {
                    Hide();
                }

                this.BodyEnteredAsObservable()
                    .Subscribe(this, (x, s) => s.OnBodyEntered(x))
                    .AddTo(this);
                _deadSubject.AddTo(this);
                SetProcess(true);
                break;
            }
            case NotificationProcess:
            {
                // 寿命を増加させる
                Age++;

                if (Age > FirstSleepFrames)
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

                // 継続ダメージ処理
                if (Age >= FirstSleepFrames && DamageEveryXFrames > 0 && Age % DamageEveryXFrames == 0)
                {
                    OnDamageEveryXFrames();
                }

                // 寿命が 0 の場合は無限に生存するとする
                if (LifeFrame > 0)
                {
                    if (Age > LifeFrame)
                    {
                        OnDead(WhyDead.Life);
                    }
                }

                break;
            }
        }
    }

    public virtual void OnDead(WhyDead reason)
    {
        _deadSubject.OnNext(reason);
        _deadSubject.OnCompleted();

        // Physics Process から呼ばれる (衝突時死亡) ことがあるので CallDeferred で
        CallDeferred(Node.MethodName.QueueFree);
    }

    private protected virtual void OnBodyEntered(Node2D body)
    {
        if (Age < FirstSleepFrames)
        {
            return;
        }

        // 最新の HitInfo を更新
        // Note: すべての当たり判定が Sphere という決め打ちで法線を計算しています
        HitInfo = new ProjectileHitInfo
        {
            Node = body,
            Position = GlobalPosition,
            Normal = (GlobalPosition - body.GlobalPosition).Normalized(),
            Velocity = Direction.Normalized() * Speed
        };

        if (body is StaticBody2D staticBody)
        {
            // 壁など静的なオブジェクトとの衝突時の処理
            if (OnCollisionDie)
            {
                OnDead(WhyDead.CollidedWithWall);
            }

            return;
        }

        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);

            if (_hitSound != null)
            {
                SoundManager.PlaySoundEffect(_hitSound);
            }

            if (OnCollisionDie)
            {
                OnDead(WhyDead.CollidedWithEnemy);
            }
        }
    }

    private protected virtual void OnDamageEveryXFrames()
    {
        var bodies = GetOverlappingBodies();

        if (bodies.Count == 0)
        {
            return;
        }

        foreach (var body in bodies)
        {
            if (body is Enemy enemy)
            {
                enemy.TakeDamage(Damage);
            }
        }
    }
}