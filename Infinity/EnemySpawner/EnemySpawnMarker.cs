using Godot;

namespace fms;

public partial class EnemySpawnMarker : Node2D
{
    [Export]
    private TextureRect _textureRect = null!;

    private bool _isBlinking;

    private uint _timer;

    public PackedScene EnemyScene { get; set; } = null!;

    public uint LifeTime { get; set; }

    public Node EnemeySpawnParent { get; set; } = null!;

    public override void _Ready()
    {
        var tween = CreateTween();
        tween.TweenProperty(_textureRect, "modulate", new Color(1, 1, 1), 0.15d);
    }

    public override void _Process(double delta)
    {
        _timer++;

        if (!_isBlinking && LifeTime - _timer < 30) // 0.5f
        {
            _isBlinking = true;
            var tween = CreateTween();
            tween.TweenProperty(_textureRect, "modulate", new Color(1, 1, 1, 0), 0.1d);
            tween.TweenProperty(_textureRect, "modulate", new Color(1, 1, 1), 0.1d);
            tween.SetLoops(2);
            tween.TweenProperty(_textureRect, "modulate", new Color(1, 1, 1, 0), 0.1d);
        }

        if (_timer >= LifeTime)
        {
            var enemy = EnemyScene.Instantiate<Node2D>();
            enemy.GlobalPosition = GlobalPosition;
            EnemeySpawnParent.AddChild(enemy);

            QueueFree();
        }
    }
}