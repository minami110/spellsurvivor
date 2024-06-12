using System.Globalization;
using Godot;

namespace fms;

public partial class DamageNumberHud : Node2D
{
    [Export]
    private Label _damageLabel = null!;

    private int _lifeFrameCounter;

    public float Damage { get; set; }

    public Color Color { get; set; }
    private int LifeFrame { get; set; } = 20;

    public override void _Ready()
    {
        _damageLabel.Text = Damage.ToString(CultureInfo.InvariantCulture);
        _damageLabel.Modulate = Color;
        _lifeFrameCounter = LifeFrame;

        var tween = CreateTween();

        var scale = Damage switch
        {
            >= 200 => 2f,
            >= 50 => 1.5f,
            _ => 1.2f
        };

        tween.TweenProperty(this, "scale", new Vector2(scale, scale), 0.08d);
    }

    public override void _Process(double _)
    {
        _lifeFrameCounter--;
        if (_lifeFrameCounter <= 0)
        {
            QueueFree();
        }
    }
}