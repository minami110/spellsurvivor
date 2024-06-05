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
    private float _coolDown = 0.333f;

    [ExportGroup("Internal References")]
    [Export]
    private TextureRect _mainTexture = null!;

    [Export]
    private TextureProgressBar _progressBar = null!;

    [Export]
    private Area2D _damageArea = null!;

    private readonly EnemyState _state = new();

    private float _coolDownTimer;

    private Vector2? _targetPosition;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _coolDownTimer = _coolDown;
        _state.AddTo(this);

        // Init state
        _state.AddEffect(new AddMaxHealthEffect { Value = _defaultHealth });
        _state.AddEffect(new AddHealthEffect { Value = _defaultHealth });
        _state.AddEffect(new AddMoveSpeedEffect { Value = _defaultMoveSpeed });
        _state.SolveEffect();

        // Refresh HUD
        UpdateHealthBar();

        // Gets the player's position
        var player = GetNodeOrNull<Node2D>("%Player");
        if (player != null)
        {
            _targetPosition = player.GlobalPosition;
            SetProcess(true);
            SetPhysicsProcess(true);
        }
        else
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = _targetPosition!.Value - GlobalPosition;
        direction = direction.Normalized();
        var force = direction * _state.MoveSpeed.CurrentValue;
        LinearVelocity = force;
    }

    public override void _Process(double delta)
    {
        if (_coolDownTimer > 0)
        {
            _coolDownTimer -= (float)delta;
            return;
        }

        _coolDownTimer = _coolDown;
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

    private void KillByDamage()
    {
        QueueFree();
    }

    /// <summary>
    ///     Wave 終了時に GameMode から呼ばれる
    /// </summary>
    private void KillByWaveEnd()
    {
        QueueFree();
    }

    private void TakeDamageAnimationAsync()
    {
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