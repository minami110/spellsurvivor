using Godot;

namespace fms;

public partial class MeMeAnimator : BasePlayerAnimator
{
    [Export]
    private Sprite2D _body = null!;

    [Export]
    private Sprite2D _head = null!;

    [Export]
    private Sprite2D _footL = null!;

    [Export]
    private Sprite2D _footR = null!;

    private Tween _movingFootTween = null!;

    public override void _Ready()
    {
        var t = CreateTween();
        t.TweenProperty(_head, "position", new Vector2(0, 1), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t.TweenProperty(_head, "position", new Vector2(0, 0), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t.SetLoops();

        _movingFootTween = CreateTween();
        _movingFootTween.SetParallel();
        _movingFootTween.TweenProperty(_footR, "position", new Vector2(3, 0), 0.1);
        _movingFootTween.TweenProperty(_footL, "position", new Vector2(0, 0), 0.1);
        _movingFootTween.Chain();
        _movingFootTween.TweenProperty(_footR, "position", new Vector2(0, 0), 0.1);
        _movingFootTween.TweenProperty(_footL, "position", new Vector2(3, 0), 0.1);
        _movingFootTween.SetLoops();
        _movingFootTween.Stop();
    }

    private protected override void SendSignalMove(double delta)
    {
        _movingFootTween.Play();
    }

    private protected override void SendSignalMoveLeft()
    {
        _body.Scale = new Vector2(-3, 3);
    }

    private protected override void SendSignalMoveRight()
    {
        _body.Scale = new Vector2(3, 3);
    }

    private protected override void SendSignalStop()
    {
        _movingFootTween.Stop();
        _footR.Position = new Vector2(0, 0);
        _footL.Position = new Vector2(3, 0);
    }
}