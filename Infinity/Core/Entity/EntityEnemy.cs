using System.IO;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public partial class EntityEnemy : RigidBody2D, IEntity
{
    /// <summary>
    /// 体力の基礎値
    /// </summary>
    [ExportGroup("Status")]
    [Export(PropertyHint.Range, "0,10000,1")]
    private protected float BaseHealth { get; private set; } = 100f;

    /// <summary>
    /// 移動速度の基礎値
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private protected float BaseSpeed { get; private set; } = 50f;

    /// <summary>
    /// 設定した速度 ± ランダム値 の振れ幅の値. 計算には正規分布を使用する
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _randomSpeed;

    /// <summary>
    /// 攻撃力の基礎値
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
    private protected float BaseDamage { get; private set; } = 10f;

    /// <summary>
    /// 現在の Enemy のLevel, Main Game では Spawner から自動で代入される
    /// </summary>
    [ExportGroup("For Developer")]
    [Export(PropertyHint.Range, "1,100,1")]
    public uint Level { get; internal set; } = 1u;

    /// <summary>
    /// 死亡時に発生させるパーティクル
    /// </summary>
    [Export]
    private PackedScene _onDeadParticle = null!;

    private uint _knockbackTimer;

    private protected Node2D _playerNode = null!;

    /// <summary>
    /// スポーンしてからの経過フレーム数
    /// </summary>
    internal uint Age { get; private set; }

    /// <summary>
    /// 現在ノックバック状態かどうか
    /// </summary>
    public bool Knockbacking => _knockbackTimer > 0u;

    /// <summary>
    /// Enemy が現在目指している方向 (Animator などから参照される)
    /// </summary>
    public Vector2 TargetVelocity { get; private protected set; }

    protected string CauserPath
    {
        get
        {
            var scenePath = GetSceneFilePath();
            // ファイル名を抽出する
            // Note: res://foo/bar/super_spider.tscn => SuperSpider 
            var fileName = Path.GetFileNameWithoutExtension(scenePath);
            return "Enemy/" + fileName.ToPascalCase();
        }
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationEnterTree:
            {
                AddToGroup(GroupNames.Enemy);
                break;
            }
            case NotificationReady:
            {
                // Player の Node をキャッシュする
                if (GetTree().GetFirstNodeInGroup(GroupNames.Player) is Node2D player)
                {
                    _playerNode = player;

                    // Note: Godot の仕様で Override してるメソッドがないと動かないので
                    //       Age の管理のためだけに手動で有効化
                    // SetProcess(true);
                    SetPhysicsProcess(true);
                }
                else
                {
                    // SetProcess(false);
                    SetPhysicsProcess(false);

                    GD.PrintErr($"[{nameof(EntityEnemy)}] Player node is not found");
                    return;
                }

                // スポーン時のパラメーターを初期化する
                InitializeParameters();

                // Refresh HUD
                UpdateHealthBar();

                // Disposable 関係
                State.AddTo(this);

                break;
            }
            case NotificationPhysicsProcess:
            {
                if (IsDead)
                {
                    return;
                }

                // 毎フレーム Age を加算させる
                Age++;
                // ToDo: すべての Enemy 共通で雑にスタンの処理を書いています
                if (_knockbackTimer > 0u)
                {
                    _knockbackTimer--;
                    if (_knockbackTimer == 0u)
                    {
                        OnEndKnockback();
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// ノックバックを適用する
    /// </summary>
    /// <param name="impulse"></param>
    public virtual void ApplyKnockback(in Vector2 impulse)
    {
        // すでにノックバック中 あるいは power が 0 以下のときは処理をスキップする
        if (impulse.LengthSquared() <= 0f || Knockbacking)
        {
            return;
        }

        if (IsDead)
        {
            // すでに死亡しているときは Mass をあまり考慮しない感じで飛んでほしいので, 武器の Knockback 値をそのまま反映させる
            // 感じにぶっ飛ばす.
            // またこのあと Tree から削除されるのでいろいろな処理が不要
            LinearVelocity = (float)GD.Randfn(6f, 3f) * impulse;

            return;
        }

        // 他の動きを停止するため, KnockBackTimer を設定する
        _knockbackTimer = 15u; // frames

        // Note: 既存の Linear Velocity をリセットして新しい値を適用する
        //       ApplyCentralImpulse と同じ処理の上書き版を書いている
        LinearVelocity = impulse / Mass; // Note: Mass の設定値に応じて実際の動き度が変わるので要注意

        // Note: Knockback 中は他の敵を押しのけて欲しいので, 一時的に CollisionMask を無効化する
        CollisionMask = Constant.LAYER_NONE;
    }

    private protected virtual void KillByDamage()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        // 自身の物理を無効化する
        GetNode<CollisionShape2D>("CollisionForRigidbody").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        // 移動速度をゼロに
        LinearVelocity = Vector2.Zero;

        // Emit Dead Particle
        var onDeadParticle = _onDeadParticle.Instantiate<GpuParticles2D>();
        onDeadParticle.GlobalPosition = GlobalPosition;
        GetParent().CallDeferred(Node.MethodName.AddChild, onDeadParticle);
        onDeadParticle.Emitting = true;

        // Scale を小さく, くるくる回転させる Tween を再生する
        // Note: sprite をいじる, this を対象にすると Knockback がのらなくなる
        var sprite = GetNode<Sprite2D>("Sprite");
        var tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(sprite, "scale", new Vector2(0f, 0f), 0.5d)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        tween.TweenProperty(sprite, "rotation", GD.Randfn(8f, 3f), 0.5d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);

        // Tweeen の終了時に Free を呼ぶ
        tween.Finished += () =>
        {
            // Particle が勝手に消えないのでついでに殺す (ToDo: 将来 Pool にすること) 
            onDeadParticle.CallDeferred(GodotObject.MethodName.Free);
            CallDeferred(GodotObject.MethodName.Free);
        };
    }

    private protected void OnEndKnockback()
    {
        // Knockback 終了時に Mass を元に戻す
        CollisionMask = Constant.LAYER_MOB;
    }

    /// <summary>
    /// スポーン時の体力や移動速度などの初期化を行う
    /// </summary>
    private void InitializeParameters()
    {
        // ToDo: すべての Enemy 共通で雑にレベルでスケールする設定になっています
        //       (Base が 10 のとき) Lv.1 : 10, Lv.2 : 15, Lv.3 : 20, ...
        var health = (uint)(BaseHealth + (Level - 1) * 5f);
        var speed = (uint)GD.Randfn(BaseSpeed, _randomSpeed);

        State = new EntityState(0u, health, speed, 0u);
    }

    /// <summary>
    /// ウェーブ終了時などシステムに殺されるときに呼ばれる処理
    /// </summary>
    private void KillByWaveEnd()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        // QueueFree
        CallDeferred(GodotObject.MethodName.Free);
    }

    private void OnTakeDamage(float amount, IEntity instigator, Node causer, string causerPath)
    {
        // 体力が 0 以下になったら死亡
        if (State.Health.CurrentValue <= 0)
        {
            var report = new DamageReport
            {
                Instigator = instigator,
                Victim = this,
                Amount = amount,
                Position = GlobalPosition,
                Causer = causer,
                CauserPath = causerPath,
                IsVictimDead = true
            };
            StaticsManager.ReportDamage(report);
            KillByDamage();
        }
        // まだ体力が残っているとき
        else
        {
            var report = new DamageReport
            {
                Instigator = instigator,
                Victim = this,
                Amount = amount,
                Position = GlobalPosition,
                Causer = causer,
                CauserPath = causerPath,
                IsVictimDead = false
            };
            StaticsManager.ReportDamage(report);
            TakeDamageAnimationAsync();
            UpdateHealthBar();
        }
    }

    private void TakeDamageAnimationAsync()
    {
        // Hitstop and blink shader
        var tween = CreateTween();
        tween.TweenMethod(Callable.From((float value) => UpdateShaderParameter(value)), 0f, 1f, 0.05f);
        tween.TweenMethod(Callable.From((float value) => UpdateShaderParameter(value)), 1f, 0f, 0.05f);
    }

    private void UpdateHealthBar()
    {
        var healthBar = GetNodeOrNull<Range>("HealthBar");
        if (healthBar is null)
        {
            return;
        }

        healthBar.MaxValue = State.Health.MaxValue;
        healthBar.SetValueNoSignal(State.Health.CurrentValue);
    }

    private void UpdateShaderParameter(float value)
    {
        if (GetNode<CanvasItem>("%Sprite").Material is not ShaderMaterial sm)
        {
            return;
        }

        sm.SetShaderParameter("hit", value);
    }

    /// <summary>
    /// 現在の State
    /// </summary>
    public EntityState State { get; private set; } = null!;

    /// <summary>
    /// 現在死亡しているかどうか
    /// </summary>
    public bool IsDead { get; private set; }

    /// <summary>
    /// プレイヤーなどからダメージを受けるときの処理
    /// </summary>
    void IEntity.ApplayDamage(float amount, IEntity instigator, Node causer, string causerPath)
    {
        if (IsDead || amount.Equals(0f))
        {
            return;
        }

        State.ApplyDamage((uint)amount);
        OnTakeDamage(amount, instigator, causer, causerPath);
    }

    Vector2 IEntity.Position => GlobalPosition;

    void IGodotNode.AddChild(Node node)
    {
        AddChild(node);
    }

    void IGodotNode.RemoveChild(Node node)
    {
        RemoveChild(node);
    }
}