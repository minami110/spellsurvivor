using Godot;

namespace fms;

public partial class PlayerController : Node
{
    [Export]
    private Node2D? _customPawn;

    private static readonly StringName InputMoveRight = "move_right";
    private static readonly StringName InputMoveLeft = "move_left";
    private static readonly StringName InputMoveUp = "move_up";
    private static readonly StringName InputMoveDown = "move_down";
    private static readonly StringName InputPrimary = "primary";

    private IPawn? _posessedPawn;

    public override void _EnterTree()
    {
        SetProcess(false);
    }

    public override void _Ready()
    {
        if (_customPawn is null)
        {
            // Pawn が指定されていない場合, ツリー内に存在する "Player" グループに所属する最初のノードを取得する
            if (GetTree().GetFirstNodeInGroup("Player") is Node2D player)
            {
                _customPawn = player;
            }
        }

        if (_customPawn is IPawn pawn)
        {
            Possess(pawn);
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (_posessedPawn is null)
        {
            return;
        }

        if (inputEvent.IsActionPressed(InputPrimary))
        {
            _posessedPawn.PrimaryPressed();
        }
        else if (inputEvent.IsActionReleased(InputPrimary))
        {
            _posessedPawn.PrimaryReleased();
        }
    }

    public override void _Process(double delta)
    {
        if (_posessedPawn is null)
        {
            return;
        }

        // Polling the input axes.
        var velocity = Vector2.Zero; // The player's movement vector.
        velocity.X = Input.GetAxis(InputMoveLeft, InputMoveRight);
        velocity.Y = Input.GetAxis(InputMoveUp, InputMoveDown);

        // Normalize the velocity vector.
        var direction = velocity.Normalized();
        var length = Mathf.Min(1.0f, velocity.Length()); // Clamp the length to 1.0f.

        _posessedPawn.MoveForward(direction * length);
    }

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
}