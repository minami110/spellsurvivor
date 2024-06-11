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

    public override void _Input(InputEvent inputEvent)
    {
        if (_posessedPawn is null)
        {
            // Note: 入力があるたびに Player タグがシーン内に存在するかを検索する
            if (TryPossesFromPlayerTag() == false)
            {
                return;
            }
        }
        else
        {
            if (!IsInstanceValid((Node)_posessedPawn))
            {
                Unpossess();
                this.DebugLog("Invalid pawn detected. unposssed");
                return;
            }
        }

        var pawn = _posessedPawn!;
        if (inputEvent.IsActionPressed(InputPrimary))
        {
            pawn.PrimaryPressed();
        }
        else if (inputEvent.IsActionReleased(InputPrimary))
        {
            pawn.PrimaryReleased();
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
    private bool Possess(IPawn pawn)
    {
        if (_posessedPawn is not null)
        {
            return false;
        }

        _posessedPawn = pawn;
        SetProcess(true);
        return true;
    }

    private bool TryPossesFromPlayerTag()
    {
        // Pawn が指定されていない場合, ツリー内に存在する "Player" グループに所属する最初のノードを取得する
        var node = GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayer);
        return node is IPawn pawn && Possess(pawn);
    }

    /// <summary>
    /// </summary>
    private void Unpossess()
    {
        _posessedPawn = null;
        SetProcess(false);
    }
}