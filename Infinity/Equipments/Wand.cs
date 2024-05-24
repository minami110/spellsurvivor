using Godot;

namespace fms;

public interface IEquipment
{
    public void PrimaryPress();
    public void PrimaryRelease();
}

public partial class Wand : Area2D, IEquipment
{
    [Export]
    private PackedScene _projectileScene = null!;

    void IEquipment.PrimaryPress()
    {
        var mousePosition = GetGlobalMousePosition();
        var projectileSpawnPosition = GetNode<Marker2D>("ProjectileSpawnPoint").GlobalPosition;

        // Instantiate Projectile
        var projectile = _projectileScene.Instantiate<Projectile>();
        {
            projectile.Instigator = GetNode<IEntity>("../");
            projectile.InitialSpeed = 500f;
            projectile.Acceleration = 1000f;
            projectile.Direction = (mousePosition - GlobalPosition).Normalized();
        }

        // Attach Root
        GetTree().Root.AddChild(projectile);
        projectile.GlobalPosition = projectileSpawnPosition;
    }

    void IEquipment.PrimaryRelease()
    {
        GD.Print("Wand released");
    }

    public override void _Process(double delta)
    {
        // Rotate this to face mouse
        var mousePosition = GetGlobalMousePosition();
        var direction = (mousePosition - GlobalPosition).Normalized();
        Rotation = direction.Angle();
    }
}