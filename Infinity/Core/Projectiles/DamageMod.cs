using Godot;

namespace fms.Projectile;

public partial class DamageMod : Node
{
    public required int Add { get; init; }
    public required float Multiply { get; init; }

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        projectile.Damage = (projectile.Damage + Add) * Multiply;
    }
}