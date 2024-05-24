using Godot;
using R3;

namespace spellsurvivor;

public partial class Player : Area2D, IPawn, IEntity
{
    private readonly Subject<DeadReason> _deadSubject = new();

    [Export] private ProgressBar _healthBar = null!;

    private Vector2 _nextMoveDirection;

    private Vector2 _screenSize;

    [Export] public float MoveSpeed { get; private set; } = 400;

    public Observable<DeadReason> Dead => _deadSubject;

    [Export(PropertyHint.Range, "0, 100")] public float Health { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0, 100")] public float MaxHealth { get; private set; } = 100f;

    public Race Race => Race.Player;

    void IEntity.TakeDamage(float amount, IEntity? instigator)
    {
        if (instigator is null)
        {
            return;
        }

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            // Emit signal to main scene
            _deadSubject.OnNext(new DeadReason(instigator.Race.ToString(), "Attack"));
        }

        // Update Health bar
        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = Health;
    }

    void IPawn.PrimaryPressed()
    {
        var equipment = GetNodeOrNull<IEquipment>("Equipment");
        if (equipment is null)
        {
            GD.Print("Equipment is missing");
            return;
        }

        equipment.PrimaryPress();
    }

    void IPawn.PrimaryReleased()
    {
    }

    void IPawn.MoveForward(in Vector2 dir)
    {
        _nextMoveDirection = dir;
    }

    public override void _ExitTree()
    {
        _deadSubject.Dispose();
    }

    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;

        // Update Health bar
        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = Health;
    }

    public override void _Process(double delta)
    {
        if (_nextMoveDirection.LengthSquared() > 0f)
        {
            Position += _nextMoveDirection * (float)delta * MoveSpeed;
            _nextMoveDirection = Vector2.Zero;
        }
    }
}