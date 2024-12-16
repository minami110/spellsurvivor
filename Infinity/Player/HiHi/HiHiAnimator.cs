using Godot;

namespace fms;

public partial class HiHiAnimator : BasePlayerAnimator
{
    [Export]
    private Sprite2D _body = null!;

    [Export]
    private Sprite2D _head = null!;

    [Export]
    private Node2D _footL = null!;

    [Export]
    private Node2D _footR = null!;

    [Export]
    private Node2D _arm = null!;


    private Tween _movingFootTween = null!;

    public override async void _Ready()
    {
        var t2 = CreateTween();
        t2.TweenProperty(_arm, "rotation", Mathf.DegToRad(3), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t2.TweenProperty(_arm, "rotation", Mathf.DegToRad(-2), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t2.SetLoops();

        var t3 = CreateTween();
        t3.TweenProperty(_body, "position", new Vector2(0, 0), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t3.TweenProperty(_body, "position", new Vector2(0, -1), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t3.SetLoops();

        _movingFootTween = CreateTween();
        _movingFootTween.SetParallel();
        _movingFootTween.TweenProperty(_footL, "rotation", Mathf.DegToRad(60), 0.2);
        _movingFootTween.TweenProperty(_footR, "rotation", Mathf.DegToRad(-60), 0.2);
        _movingFootTween.TweenProperty(_footL, "position", new Vector2(0, 0), 0.2);
        _movingFootTween.Chain();
        _movingFootTween.TweenProperty(_footL, "rotation", 0f, 0.2);
        _movingFootTween.TweenProperty(_footR, "rotation", 0f, 0.2);
        _movingFootTween.TweenProperty(_footL, "position", new Vector2(10, 0), 0.2);
        _movingFootTween.SetLoops();
        _movingFootTween.Stop();

        await this.WaitForSecondsAsync(0.1d);

        var t = CreateTween();
        t.TweenProperty(_head, "position", new Vector2(10, -98), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t.TweenProperty(_head, "position", new Vector2(10, -103), 0.2)
            .SetTrans(Tween.TransitionType.Cubic);
        t.SetLoops();
    }

    private protected override void SendSignalMove(double delta)
    {
        _movingFootTween.Play();
    }

    private protected override void SendSignalMoveLeft()
    {
        _body.Scale = new Vector2(0.2f, 0.2f);
    }

    private protected override void SendSignalMoveRight()
    {
        _body.Scale = new Vector2(-0.2f, 0.2f);
    }

    private protected override void SendSignalStop()
    {
        _movingFootTween.Stop();
        _footR.Rotation = 0;
        _footL.Rotation = 0;
        _footL.Position = new Vector2(10, 0);
    }
}