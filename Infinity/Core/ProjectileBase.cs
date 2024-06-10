using Godot;

namespace fms.Projectile;

public partial class ProjectileBase : Node
{
    public float BaseDamage { get; set; }

    public Vector2 InitialPosition { get; set; }

    /// <summary>
    ///     Projectile の寿命
    /// </summary>
    public int LifeFrame { get; set; } = 120;

    public int CurrentFrame { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            if (!IsInGroup(Constant.GroupNameProjectile))
            {
                AddToGroup(Constant.GroupNameProjectile);
            }
        }
        else if (what == NotificationProcess)
        {
            CurrentFrame++;
            if (CurrentFrame > LifeFrame)
            {
                KillThis();
            }
        }
    }

    private protected virtual void KillThis()
    {
        QueueFree();
    }

    private protected void ResetLifeFrameCounter()
    {
        CurrentFrame = 0;
    }
}