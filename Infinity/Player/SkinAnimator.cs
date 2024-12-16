using Godot;

namespace fms;

public partial class SkinAnimator : BasePlayerAnimator
{
    private static readonly StringName TrackNameIdle = new("idle");
    private static readonly StringName TrackNameRunning = new("running");
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
        _player.Play(TrackNameRunning);

        var currentSpeed = LinearVelocity.Length() / (float)delta;
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