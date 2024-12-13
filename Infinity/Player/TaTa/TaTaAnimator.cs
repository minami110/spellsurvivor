using Godot;

namespace fms;

public partial class TaTaAnimator : BasePlayerAnimator
{
    private AnimatedSprite2D _sprite = null!;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("%Sprite");
        _sprite.Play("default");
    }

    private protected override void SendSignalMove()
    {
        _sprite.Play("run");
    }

    private protected override void SendSignalMoveLeft()
    {
        _sprite.FlipH = true;
    }

    private protected override void SendSignalMoveRight()
    {
        _sprite.FlipH = false;
    }

    private protected override void SendSignalStop()
    {
        _sprite.SetAnimation("default");
    }
}