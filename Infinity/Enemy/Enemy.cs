using Godot;
using R3;

namespace fms;

public partial class Enemy : RigidBody2D
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _defaultMoveSpeed = 50f;

    [Export(PropertyHint.Range, "0,10000,1")]
    private float _defaultHealth = 100f;

    /// <summary>
    ///     プレイヤーに与えるダメージ
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _power = 10f;

    /// <summary>
    ///     プレイヤーと重なっている時攻撃を発生させるクールダウン
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    private uint _coolDownFrame = 20;

    [Export]
    private PackedScene _onDeadParticle = null!;

    private readonly EnemyState _state = new();

    private Node2D? _targetNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Gets the player's position
        if (GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayer) is Node2D player)
        {
            _targetNode = player;
        }
        else
        {
            GD.PrintErr($"[{nameof(Enemy)}] Player node is not found");
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

        // Subscribe and start FrameTimer
        var timer = new FrameTimer();
        AddChild(timer);
        var d1 = timer.TimeOut.Subscribe(_ => Attack()).AddTo(this);
        timer.WaitFrame = _coolDownFrame;
        timer.Start();

        Disposable.Combine(_state, d1).AddTo(this);
    }

    public override void _PhysicsProcess(double _)
    {
        var delta = _targetNode!.GlobalPosition - GlobalPosition;
        // 2opx 以内に近づいたら移動を停止する
        if (delta.LengthSquared() < 400)
        {
            LinearVelocity = Vector2.Zero;
            return;
        }

        var direction = delta.Normalized();
        var force = direction * _state.MoveSpeed.CurrentValue;
        LinearVelocity = force;
    }

    public void TakeDamage(float amount)
    {
        _state.AddEffect(new PhysicalDamageEffect { Value = amount });
        _state.SolveEffect();
        StaticsManager.CommitDamage(StaticsManager.DamageTakeOwner.Enemy, amount, GlobalPosition);

        if (_state.Health.CurrentValue <= 0)
        {
            // Dead
            KillByDamage();
        }
        else
        {
            TakeDamageAnimationAsync();
            UpdateHealthBar();
        }
    }

    private void Attack()
    {
        var damageArea = GetNode<Area2D>("DamageArea");
        var overlappingBodies = damageArea.GetOverlappingBodies();
        if (overlappingBodies.Count <= 0)
        {
            return;
        }

        foreach (var node in overlappingBodies)
        {
            if (node is MeMe player)
            {
                player.TakeDamage(_power);
            }
        }
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
        if (GetNode<TextureRect>("Sprite").Material is not ShaderMaterial sm)
        {
            return;
        }

        sm.SetShaderParameter("hit", value);
    }
}