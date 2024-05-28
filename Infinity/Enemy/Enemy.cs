using Godot;
using R3;

namespace fms;

public partial class Enemy : RigidBody2D, IEntity
{
    [Export(PropertyHint.Range, "0,1000,1")]
    public float MoveSpeed { get; set; } = 50f;

    [Export(PropertyHint.Range, "0,1000,1")]
    public float MaxHealth { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0,1000,1")]
    public float Health { get; private set; } = 100f;

    /// <summary>
    ///     プレイヤーに与えるダメージ
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1")]
    public float Power { get; private set; } = 10f;

    /// <summary>
    ///     プレイヤーと重なっている時攻撃を発生させるクールダウン
    /// </summary>
    [Export]
    public float AttachCooldown { get; private set; } = 0.333f;

    [ExportGroup("Internal References")]
    [Export]
    private TextureRect _mainTexture = null!;

    [Export]
    private TextureProgressBar _progressBar = null!;

    [Export]
    private Area2D _damageArea = null!;

    private readonly Subject<DeadReason> _deadSubject = new();
    private float _attachCooldownTimer;

    private bool _isHitStopping;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateHealthBar();
        _attachCooldownTimer = AttachCooldown;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isHitStopping)
        {
            return;
        }

        MoveToPlayer(delta);
    }

    public override void _Process(double delta)
    {
        if (_attachCooldownTimer > 0)
        {
            _attachCooldownTimer -= (float)delta;
            return;
        }

        _attachCooldownTimer = AttachCooldown;
        var overlappingBodies = _damageArea.GetOverlappingBodies();
        if (overlappingBodies.Count <= 0)
        {
            return;
        }

        foreach (var node in overlappingBodies)
            if (node is MeMe player)
            {
                player.TakeDamage(Power);
            }
    }

    public override void _ExitTree()
    {
        _deadSubject.Dispose();
        base._ExitTree();
    }

    private void Deth(in DeadReason reason)
    {
        _deadSubject.OnNext(reason);
        QueueFree();
    }

    private void MoveToPlayer(double delta)
    {
        var playerPosition = Main.PlayerGlobalPosition;
        var direction = playerPosition - GlobalPosition;
        direction = direction.Normalized();
        var force = direction * MoveSpeed;
        LinearVelocity = force;
    }

    private async void TakeDamageAnimationAsync()
    {
        if (_mainTexture.Material is not ShaderMaterial sm)
        {
            return;
        }

        // Hitstop and blink shader
        sm.SetShaderParameter("hit", 1.0f);
        _isHitStopping = true;
        LinearVelocity = Vector2.Zero;

        await this.WaitForSecondsAsync(0.1f);

        _isHitStopping = false;
        sm.SetShaderParameter("hit", 0.0f);
    }

    private void UpdateHealthBar()
    {
        _progressBar.MaxValue = MaxHealth;
        _progressBar.SetValueNoSignal(Health);
    }

    public Observable<DeadReason> Dead => _deadSubject;

    public Race Race => Race.Slime;

    void IEntity.TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            Deth(new DeadReason("N/A", "Projectile"));
        }
        else
        {
            TakeDamageAnimationAsync();
            UpdateHealthBar();
        }
    }
}