using Godot;

public partial class BasePlayerAnimator : Node
{
    private Vector2? _prevPosition;

    public override void _Process(double delta)
    {
        if (_prevPosition.HasValue)
        {
            var currentPosition = GetParent<Node2D>().GlobalPosition;
            var vel = currentPosition - _prevPosition.Value;
            if (vel.LengthSquared() <= 0f)
            {
                SendSignalStop();
            }
            else
            {
                SendSignalMove();
                switch (vel.X)
                {
                    case > 0:
                        SendSignalMoveRight();
                        break;
                    case < 0:
                        SendSignalMoveLeft();
                        break;
                }
            }
        }

        _prevPosition = GetParent<Node2D>().GlobalPosition;
    }

    private protected virtual void SendSignalMove()
    {
    }

    private protected virtual void SendSignalMoveLeft()
    {
    }

    private protected virtual void SendSignalMoveRight()
    {
    }

    private protected virtual void SendSignalStop()
    {
    }
}