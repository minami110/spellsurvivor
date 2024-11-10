using Godot;
using R3;

namespace fms;

public partial class EnemyBase : RigidBody2D, IEntity
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
    private float _randomSpeed = 0f;

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

    /// <summary>
    /// 現在の State
    /// </summary>
    internal readonly EnemyState State = new();

    /// <summary>
    /// スポーンしてからの経過フレーム数
    /// </summary>
    internal uint Age { get; private set; }

    /// <summary>
    /// 現在ノックバック状態かどうか
    /// </summary>
    private protected bool Knockbacking => _knockbackTimer > 0u;

    private protected Node2D _playerNode = null!;

    private uint _knockbackTimer = 0u;

    private float _defaultMass;

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationEnterTree:
            {
                AddToGroup(Constant.GroupNameEnemy);
                break;
            }
            case NotificationReady:
            {
                // Disposable 関係
                State.AddTo(this);

                // Player の Node をキャッシュする
                if (GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayer) is Node2D player)
                {
                    _playerNode = player;

                    // Note: Godot の仕様で Override してるメソッドがないと動かないので
                    //       Age の管理のためだけに手動で有効化
                    // SetProcess(true);
                    SetPhysicsProcess(true);
                }
                else
                {
                    SetProcess(false);
                    SetPhysicsProcess(false);
                    GD.PrintErr($"[{nameof(EnemyBase)}] Player node is not found");
                    return;
                }

                // スポーン時のパラメーターを初期化する
                InitializeParameters();

                // Refresh HUD
                UpdateHealthBar();

                break;
            }
            case NotificationPhysicsProcess:
            {
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
    /// スポーン時の体力や移動速度などの初期化を行う
    /// </summary>
    private protected void InitializeParameters()
    {
        // ToDo: すべての Enemy 共通で雑にレベルでスケールする設定になっています
        //       (Base が 10 のとき) Lv.1 : 10, Lv.2 : 15, Lv.3 : 20, ...
        var health = BaseHealth + (Level - 1) * 5f;

        State.AddEffect(new AddMaxHealthEffect { Value = health });
        State.AddEffect(new AddHealthEffect { Value = health });
        State.AddEffect(new AddMoveSpeedEffect { Value = (float)GD.Randfn(BaseSpeed, _randomSpeed) });
        State.SolveEffect();
    }

    /// <summary>
    /// ノックバックを適用する
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="power"></param>
    public void ApplyKnockback(Vector2 direction, float power)
    {
        if (power <= 0f)
        {
            return;
        }

        // 他の動きを停止するため, KnockBackTimer を設定する
        _knockbackTimer = 10u; // frames

        // Note: 既存の Linear Velocity をリセットして新しい値を適用する
        //       ApplyCentralImpulse と同じ処理の上書き版を書いている
        var impulce = direction.Normalized() * power;
        LinearVelocity = impulce * Mass; // Note: Mass の設定値に応じて実際の動き度が変わるので要注意

        // Knockback 中は他の敵を押しのけて欲しいので, 一時的に Mass をめちゃくちゃ上げる
        _defaultMass = Mass;
        Mass = 1000f;
    }

    private protected void OnEndKnockback()
    {
        // Knockback 終了時に Mass を元に戻す
        Mass = _defaultMass;
    }

    /// <summary>
    /// プレイヤーなどからダメージを受けるときの処理
    /// </summary>
    void IEntity.ApplayDamage(float amount, IEntity instigator, Node causer)
    {
        if (amount.Equals(0f))
        {
            return;
        }

        State.AddEffect(new PhysicalDamageEffect { Value = amount });
        State.SolveEffect();
        OnTakeDamage(amount, instigator, causer);
    }

    private void OnTakeDamage(float amount, IEntity instigator, Node causer)
    {
        // 体力が 0 以下になったら死亡
        if (State.Health.CurrentValue <= 0)
        {
            // Dead
            var report = new DamageReport
            {
                Instigator = instigator,
                Causer = causer,
                Victim = this,
                Amount = amount,
                Position = GlobalPosition,
                IsDead = true
            };
            StaticsManager.CommitDamage(report);
            KillByDamage();
        }
        // まだ体力が残っているとき
        else
        {
            var report = new DamageReport
            {
                Instigator = instigator,
                Causer = causer,
                Victim = this,
                Amount = amount,
                Position = GlobalPosition,
                IsDead = false
            };
            StaticsManager.CommitDamage(report);
            TakeDamageAnimationAsync();
            UpdateHealthBar();
        }
    }
    
    private protected virtual void KillByDamage()
    {
        // Emit Dead Particle
        var onDeadParticle = _onDeadParticle.Instantiate<GpuParticles2D>();
        onDeadParticle.GlobalPosition = GlobalPosition;
        GetParent().CallDeferred(Node.MethodName.AddChild, onDeadParticle);
        onDeadParticle.Emitting = true;

        // QueueFree
        CallDeferred(GodotObject.MethodName.Free);
    }

    private void KillByWaveEnd()
    {
        // QueueFree
        CallDeferred(GodotObject.MethodName.Free);
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
        var healthBar = GetNode<Godot.Range>("HealthBar");
        healthBar.MaxValue = State.MaxHealth.CurrentValue;
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
}