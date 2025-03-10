using Godot;

public partial class PickableAnimator : Node
{
    [Export]
    private Sprite2D _sprite = null!;

    private Tween? _idlingLoopTween;
    private Vector2 _startScale;

    public override void _Ready()
    {
        _startScale = _sprite.Scale;

        // 最初はアイドリングアニメーションを開始
        StartIdlingTween();
    }

    public void StartIdlingTween()
    {
        if (_idlingLoopTween is not null)
        {
            return;
        }

        // アニメーションにばらつきを出すため, 0.4~0.6 の範囲のランダムな値を生成する
        var duration = 0.4d + 0.2d * GD.Randf();
        // Scale するのは Sprite だけでヨイ
        var sprite = GetNode<Sprite2D>("../Sprite");

        // Tween を再生する
        var goalScale = new Vector2(1.16f, 1.16f) * _startScale;
        var property = Control.PropertyName.Scale.ToString();
        _idlingLoopTween = CreateTween();
        _idlingLoopTween.TweenProperty(sprite, property, goalScale, duration)
            .SetTrans(Tween.TransitionType.Quart);
        _idlingLoopTween.TweenProperty(sprite, property, _startScale, duration)
            .SetTrans(Tween.TransitionType.Quart);
        _idlingLoopTween.SetLoops();
    }

    public void StopIdlingTween()
    {
        if (_idlingLoopTween is null)
        {
            return;
        }

        _idlingLoopTween.Kill();
        _idlingLoopTween = null;
    }
}