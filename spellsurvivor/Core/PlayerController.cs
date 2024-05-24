using System;
using Godot;

namespace spellsurvivor;

public interface IPawn
{
    public void PrimaryPressed();
    public void PrimaryReleased();

    public void MoveForward(in Vector2 direction);
}

public partial class PlayerController : Node
{
    private static readonly StringName InputMoveRight = "move_right";
    private static readonly StringName InputMoveLeft = "move_left";
    private static readonly StringName InputMoveUp = "move_up";
    private static readonly StringName InputMoveDown = "move_down";
    private static readonly StringName InputPrimary = "primary";

    [Export] private Node _player = null!;

    private IPawn _posessedPawn = null!;

    public override void _Ready()
    {
        if (_player is IPawn pawn)
        {
            _posessedPawn = pawn;
        }
        else
        {
            throw new ApplicationException("Player must implement IPawn interface.");
        }
    }

    public override void _Process(double delta)
    {
        // Input
        if (Input.IsActionJustPressed(InputPrimary))
        {
            _posessedPawn.PrimaryPressed();
        }
        else if (Input.IsActionJustReleased(InputPrimary))
        {
            _posessedPawn.PrimaryReleased();
        }

        //
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

        velocity = velocity.Normalized();
        _posessedPawn.MoveForward(in velocity);
    }
}