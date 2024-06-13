using Godot;

namespace fms.Projectile;

public partial class SpeedMod : Node
{
    public int Add { get; init; } = 0;
    public float Multiply { get; init; } = 1.0f;

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        projectile.Speed = (uint)((projectile.Speed + Add) * Multiply);
    }
}