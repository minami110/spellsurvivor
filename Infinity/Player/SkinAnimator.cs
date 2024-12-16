using Godot;

namespace fms;

public partial class SkinAnimator : BasePlayerAnimator
{
    private static readonly StringName TrackNameIdle = new("idle");
    private static readonly StringName TrackNameRunning = new("running");
    private static readonly StringName TrackNameWalking = new("walking");

    private Vector2 _defaultScale;
    private AnimationPlayer _player = null!;
    private Node2D _skin = null!;

    public override void _Ready()
    {
        _skin = GetNode<Node2D>("../Skin");
        _player = _skin.GetNode<AnimationPlayer>("AnimationPlayer");
        _defaultScale = _skin.Scale;
    }

    private protected override void SendSignalMove(double delta)
    {
        var currentSpeed = LinearVelocity.Length() / (float)delta;

        if (_player.HasAnimation(TrackNameWalking) && currentSpeed < 100f)
        {
            _player.Play(TrackNameWalking, 0.1d);
        }
        else
        {
            _player.Play(TrackNameRunning, 0.1d);
        }

        _player.SpeedScale = currentSpeed / 100f; // アニメーションが 100px/s 想定
    }

    private protected override void SendSignalMoveLeft()
    {
        _skin.Scale = _defaultScale * new Vector2(-1, 1);
    }

    private protected override void SendSignalMoveRight()
    {
        _skin.Scale = _defaultScale * new Vector2(1, 1);
    }

    private protected override void SendSignalStop()
    {
        _player.Play(TrackNameIdle);
    }
}