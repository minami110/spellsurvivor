using Godot;
using R3;

namespace fms;

public partial class DodgeHud : Node2D
{
    [Export]
    private Label _damageLabel = null!;

    private double LifeTime { get; set; } = 0.5d;

    public override void _Ready()
    {
        var tween = CreateTween();

        var targetScale = 1.0f;

        // First scale set to zero
        Scale = Vector2.Zero;

        var prop = Node2D.PropertyName.Scale.ToString();
        tween.TweenProperty(this, prop, new Vector2(targetScale, targetScale), LifeTime - 0.1d)
            .SetTrans(Tween.TransitionType.Spring)
            .SetEase(Tween.EaseType.Out);
        tween.TweenInterval(0.1d);
        tween.FinishedAsObservable().Take(1).Subscribe(this, (_, state) => state.QueueFree()).AddTo(this);
    }
}