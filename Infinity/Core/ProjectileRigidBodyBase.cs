using Godot;

namespace fms.Projectile;

public partial class ProjectileRigidBodyBase : RigidBody2D
{
    [Export]
    public float BaseDamage { get; set; }

    /// <summary>
    ///     Projectile の寿命 (フレーム数)
    /// </summary>
    [Export(PropertyHint.Range, "0,7200")]
    public uint LifeFrame { get; set; } = 120;

    /// <summary>
    ///     Rigidbody の初期速度
    /// </summary>
    [Export]
    public float InitialSpeed { get; set; }

    /// <summary>
    ///     Rigidbody の初期 Velocity
    /// </summary>
    [Export]
    public Vector2 InitialVelocity { get; set; }

    /// <summary>
    ///     現在の寿命 (0 からスタート / フレーム数)
    /// </summary>
    private protected uint AgeFrame { get; private set; }

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
            // 寿命が 0 の場合は無限に生存するとする
            if (LifeFrame == 0)
            {
                return;
            }

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