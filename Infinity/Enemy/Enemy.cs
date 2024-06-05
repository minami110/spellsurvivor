using Godot;
using R3;

namespace fms;

public partial class Enemy : RigidBody2D
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _defaultMoveSpeed = 50f;

    [Export(PropertyHint.Range, "0,1000,1")]
    private float _defaultHealth = 100f;

    /// <summary>
    ///     プレイヤーに与えるダメージ
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _power = 10f;

    /// <summary>
    ///     プレイヤーと重なっている時攻撃を発生させるクールダウン
    /// </summary>
    [Export]
    private float _attackCoolDown = 0.333f;

    [ExportGroup("Internal References")]
    [Export]
    private TextureRect _mainTexture = null!;

    [Export]
    private TextureProgressBar _progressBar = null!;

    [Export]
    private Area2D _damageArea = null!;

    private readonly EnemyState _state = new();

    private float _attachCooldownTimer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _attachCooldownTimer = _attackCoolDown;
        _state.AddTo(this);

        // Init state
        _state.AddEffect(new AddMaxHealthEffect { Value = _defaultHealth });
        _state.AddEffect(new AddHealthEffect { Value = _defaultHealth });
        _state.AddEffect(new AddMoveSpeedEffect { Value = _defaultMoveSpeed });
        _state.SolveEffect();

        // Refresh HUD
        UpdateHealthBar();
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveToPlayer(delta);
    }

    public override void _Process(double delta)
    {
        if (_attachCooldownTimer > 0)
        {
            _attachCooldownTimer -= (float)delta;
            return;
        }

        _attachCooldownTimer = _attackCoolDown;
        var overlappingBodies = _damageArea.GetOverlappingBodies();
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

    public void KillByDamage()
    {
        QueueFree();
    }

    /// <summary>
    ///     Wave 終了時に GameMode から呼ばれる
    /// </summary>
    public void KillByWaveEnd()
    {
        QueueFree();
    }

    public void TakeDamage(float amount)
    {
        _state.AddEffect(new PhysicalDamageEffect { Value = amount });
        _state.SolveEffect();
        NotificationManager.CommitDamage(NotificationManager.DamageTakeOwner.Enemy, amount, GlobalPosition);

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

    private void MoveToPlayer(double delta)
    {
        var playerPosition = Main.PlayerNode.GlobalPosition;
        var direction = playerPosition - GlobalPosition;
        direction = direction.Normalized();
        var force = direction * _state.MoveSpeed.CurrentValue;
        LinearVelocity = force;
    }

    private void TakeDamageAnimationAsync()
    {
        if (_mainTexture.Material is not ShaderMaterial sm)
        {
            return;
        }

        // Hitstop and blink shader
        var tween = CreateTween();
        tween.TweenMethod(Callable.From((float value) => UpdateShaderParameter(value)), 0f, 1f, 0.05f);
        tween.Chain().TweenMethod(Callable.From((float value) => UpdateShaderParameter(value)), 1f, 0f, 0.05f);
        tween.Play();
    }

    private void UpdateHealthBar()
    {
        _progressBar.MaxValue = _state.MaxHealth.CurrentValue;
        _progressBar.SetValueNoSignal(_state.Health.CurrentValue);
    }

    private void UpdateShaderParameter(float value)
    {
        if (_mainTexture.Material is not ShaderMaterial sm)
        {
            return;
        }

        sm.SetShaderParameter("hit", value);
    }
}