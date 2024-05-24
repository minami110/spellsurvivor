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
        // Bool Inputs
        if (Input.IsActionJustPressed(InputPrimary))
        {
            _posessedPawn.PrimaryPressed();
        }
        else if (Input.IsActionJustReleased(InputPrimary))
        {
            _posessedPawn.PrimaryReleased();
        }

        // Float Inputs
        var velocity = Vector2.Zero; // The player's movement vector.
        velocity.X = Input.GetAxis(InputMoveLeft, InputMoveRight);
        velocity.Y = Input.GetAxis(InputMoveUp, InputMoveDown);
        var direction = velocity.Normalized();
        var length = Mathf.Min(1.0f, velocity.Length());
        velocity = direction * length;

        _posessedPawn.MoveForward(velocity);
    }
}