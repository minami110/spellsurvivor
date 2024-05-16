#nullable enable
using System;
using System.Threading;
using Godot;
using R3;

namespace spellsurvivor;



public partial class Player : Area2D, IEntity
{
    private static readonly StringName InputMoveRight = "move_right";
    private static readonly StringName InputMoveLeft = "move_left";
    private static readonly StringName InputMoveUp = "move_up";
    private static readonly StringName InputMoveDown = "move_down";
    private static readonly StringName InputPrimary = "primary";

    private Vector2 _screenSize;

    [Export] private ProgressBar _healthBar = null!;

    [Export] public float MoveSpeed { get; private set; } = 400;

    [Export(PropertyHint.Range, "0, 100")] public float Health { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0, 100")] public float MaxHealth { get; private set; } = 100f;

    public Race Race => Race.Player;

    public Observable<Unit> Dead => _deadSubject;

    private readonly Subject<Unit> _deadSubject = new();

    public override void _ExitTree()
    {
        _deadSubject.Dispose();
    }

    void IEntity.TakeDamage(float amount, IEntity? instigator)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            // Emit signal to main scene
            _deadSubject.OnNext(Unit.Default);
        }

        // Update Health bar
        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = Health;
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
        if (Input.IsActionJustPressed(InputPrimary))
        {
            OnPrimaryPressed();
        }
        else if (Input.IsActionJustReleased(InputPrimary))
        {
            OnPrimaryReleased();
        }

        ProcessMovement(delta);
    }

    private void OnPrimaryPressed()
    {
        var equipment = GetNodeOrNull<IEquipment>("Equipment");
        if (equipment is null)
        {
            GD.Print("Equipment is missing");
            return;
        }

        equipment.PrimaryPress();
    }

    private void OnPrimaryReleased()
    {
    }

    private void ProcessMovement(double delta)
    {
        var velocity = Vector2.Zero; // The player's movement vector.

        if (Input.IsActionPressed(InputMoveRight))
        {
            velocity.X += 1;
        }

        if (Input.IsActionPressed(InputMoveLeft))
        {
            velocity.X -= 1;
        }

        if (Input.IsActionPressed(InputMoveDown))
        {
            velocity.Y += 1;
        }

        if (Input.IsActionPressed(InputMoveUp))
        {
            velocity.Y -= 1;
        }

        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * MoveSpeed;
            Position += velocity * (float)delta;
        }
    }
}