using Godot;
using R3;

namespace fms;

public partial class EnemyBase : RigidBody2D, IEntity
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _defaultMoveSpeed = 50f;

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

                // Init state
                _state.AddEffect(new AddMaxHealthEffect { Value = _defaultHealth });
                _state.AddEffect(new AddHealthEffect { Value = _defaultHealth });
                _state.AddEffect(new AddMoveSpeedEffect { Value = _defaultMoveSpeed });
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

    /// <summary>
    /// クールダウンが完了したときに呼ばれる攻撃実行のコールバック
    /// </summary>
    private protected virtual void Attack()
    {
    }

    private void KillByDamage()
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
        var healthBar = GetNode<Range>("HealthBar");
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