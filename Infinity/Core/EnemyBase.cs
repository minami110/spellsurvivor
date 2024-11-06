using Godot;

namespace fms;

public partial class EnemyBase : RigidBody2D, IEntity
{
    [Export(PropertyHint.Range, "1,100,1")]
    public uint Level {get; set;} = 1u;

    [Export(PropertyHint.Range, "0,1000,1")]
    private float _defaultMoveSpeed = 50f;

    /// <summary>
    /// 設定した速度 ± ランダム値 の振れ幅の値. 計算には正規分布を使用する
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
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

    private protected readonly EnemyState _state = new();

    private protected Node2D? _playerNode;

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
                // Player の Node をキャッシュする
                if (GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayer) is Node2D player)
                {
                    _playerNode = player;
                }
                else
                {
                    GD.PrintErr($"[{nameof(EnemyBase)}] Player node is not found");
                    SetProcess(false);
                    SetPhysicsProcess(false);
                    return;
                }

                // ToDo: すべての Enemy 共通で雑にレベルでスケールする設定になっています
                //       (Base が 10 のとき) Lv.1 : 10, Lv.2 : 15, Lv.3 : 20, ...
                var health = _defaultHealth + (Level - 1) * 5f;

                _state.AddEffect(new AddMaxHealthEffect { Value = health });
                _state.AddEffect(new AddHealthEffect { Value = health });
                _state.AddEffect(new AddMoveSpeedEffect { Value = (float)GD.Randfn(_defaultMoveSpeed, _randomSpeed) });
                _state.SolveEffect();

                // Refresh HUD
                UpdateHealthBar();

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

        _state.AddEffect(new PhysicalDamageEffect { Value = amount });
        _state.SolveEffect();

        if (_state.Health.CurrentValue <= 0)
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
        healthBar.MaxValue = _state.MaxHealth.CurrentValue;
        healthBar.SetValueNoSignal(_state.Health.CurrentValue);
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