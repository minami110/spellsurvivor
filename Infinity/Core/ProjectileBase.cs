﻿using Godot;

namespace fms.Projectile;

public partial class ProjectileBase : Node
{
    [Export]
    public float BaseDamage { get; set; }

    /// <summary>
    ///     Projectile の寿命 (フレーム数)
    /// </summary>
    [Export]
    public uint LifeFrame { get; set; } = 120;

    public Vector2 InitialPosition { get; set; }

    /// <summary>
    /// 現在の寿命 (0 からスタート / フレーム数)
    /// </summary>
    protected uint AgeFrame { get; private set; }

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
            AgeFrame++;
            if (AgeFrame > LifeFrame)
            {
                KillThis();
            }
        }
    }

    private protected virtual void KillThis()
    {
        QueueFree();
    }

    private protected void ResetAge()
    {
        AgeFrame = 0;
    }
}