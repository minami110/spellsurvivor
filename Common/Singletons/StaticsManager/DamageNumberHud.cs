using System.Globalization;
using Godot;
using R3;

namespace fms;

public partial class DamageNumberHud : Node2D
{
    [Export]
    private Label _damageLabel = null!;

    public float Damage { get; set; }

    public Color HealColor { get; set; }

    public Color PhysicalDamageColor { get; set; }

    private double LifeTime { get; set; } = 0.5d;

    public override void _Ready()
    {
        if (Damage < 0)
        {
            // 回復のときは反転させる
            _damageLabel.Modulate = HealColor;
            var d = -Damage;
            _damageLabel.Text = $"+{d.ToString(CultureInfo.InvariantCulture)}";
        }
        else
        {
            _damageLabel.Modulate = PhysicalDamageColor;
            _damageLabel.Text = Damage.ToString(CultureInfo.InvariantCulture);
        }

        var tween = CreateTween();

        var scale = Damage switch
        {
            >= 200 => 2f,
            >= 50 => 1.5f,
            >= 10 => 1.2f,
            <= 0 => 1.5f,
            _ => 1.0f
        };

        // First scale set to zero
        Scale = Vector2.Zero;

        var prop = Node2D.PropertyName.Scale.ToString();
        tween.TweenProperty(this, prop, new Vector2(scale, scale), LifeTime - 0.1d)
            .SetTrans(Tween.TransitionType.Spring)
            .SetEase(Tween.EaseType.Out);
        tween.TweenInterval(0.1d);
        tween.FinishedAsObservable().Take(1).Subscribe(this, (_, state) => state.QueueFree()).AddTo(this);
    }
}