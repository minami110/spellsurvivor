using Godot;
using R3;

namespace fms;

public partial class EnemyBase : RigidBody2D, IEntity
{
    [Export(PropertyHint.Range, "1,100,1")]
    public uint Level {get; set;} = 1u;

    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _defaultMoveSpeed = 50f;

    /// <summary>
    /// 設定した速度 ± ランダム値 の振れ幅の値. 計算には正規分布を使用する
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _randomSpeed = 0f;

    [Export(PropertyHint.Range, "0,10000,1")]
    private float _defaultHealth = 100f;

    /// <summary>
    ///     プレイヤーに与えるダメージ
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
    private protected float _power = 10f;

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

    private protected Node2D _playerNode = null!;

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

                // ToDo: すべての Enemy 共通で雑にレベルでスケールする設定になっています
                //       (Base が 10 のとき) Lv.1 : 10, Lv.2 : 15, Lv.3 : 20, ...
                var health = _defaultHealth + (Level - 1) * 5f;

                State.AddEffect(new AddMaxHealthEffect { Value = health });
                State.AddEffect(new AddHealthEffect { Value = health });
                State.AddEffect(new AddMoveSpeedEffect { Value = (float)GD.Randfn(_defaultMoveSpeed, _randomSpeed) });
                State.SolveEffect();

                // Refresh HUD
                UpdateHealthBar();

                break;
            }
            case NotificationPhysicsProcess:
            {
                Age++;
                break;
            }
        }
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