using Godot;

namespace fms.Projectile;

public partial class ProjectileBase : Node
{
    private int _frameCounter;
    public float BaseDamage { get; set; }

    public Vector2 InitialPosition { get; set; }

    public int LifeFrame { get; set; } = 120;

    public override void _Process(double delta)
    {
        _frameCounter++;
        if (_frameCounter > LifeFrame)
        {
            KillThis();
        }
    }

    private protected virtual void KillThis()
    {
        QueueFree();
    }

    private protected void ResetLifeFrameCounter()
    {
        _frameCounter = 0;
    }
}