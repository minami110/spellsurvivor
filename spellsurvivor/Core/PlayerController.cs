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

    private IPawn? _posessedPawn;

    /// <summary>
    /// </summary>
    /// <param name="pawn"></param>
    public void Possess(IPawn pawn)
    {
        _posessedPawn = pawn;
        SetProcess(true);
    }

    /// <summary>
    /// </summary>
    public void Unpossess()
    {
        _posessedPawn = null;
        SetProcess(false);
    }

    public override void _EnterTree()
    {
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (_posessedPawn is null)
        {
            return;
        }

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