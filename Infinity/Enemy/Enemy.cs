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

    [ExportGroup("Internal References")]
    [Export]
    private TextureRect _mainTexture = null!;

    [Export]
    private TextureProgressBar _progressBar = null!;

    [Export]
    private Area2D _damageArea = null!;

    [Export]
    private FrameTimer _attackTimer = null!;

    [Export]
    private GpuParticles2D _emitter = null!;

    private readonly EnemyState _state = new();

    private bool _isDead;

    private Node2D? _targetNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Gets the player's position
        if (GetTree().GetFirstNodeInGroup("Player") is Node2D player)
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
        var d1 = _attackTimer.TimeOut.Subscribe(_ => Attack()).AddTo(this);
        _attackTimer.WaitFrame = _coolDownFrame;
        _attackTimer.Start();

        Disposable.Combine(_state, d1).AddTo(this);
    }

    public override void _PhysicsProcess(double _)
    {
        if (_isDead)
        {
            return;
        }

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
        if (_isDead)
        {
            return;
        }

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

    private async void KillByDamage()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        // Hide components
        _mainTexture.Hide();
        _progressBar.Hide();
        _damageArea.Hide();

        // Emit Partivle
        _emitter.Restart();

        await this.WaitForSecondsAsync(1f);

        QueueFree();
    }

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