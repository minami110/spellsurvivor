using Godot;

namespace spellsurvivor;

public interface IEquipment
{
    public void PrimaryPress();
    public void PrimaryRelease();
}

public partial class Wand : Area2D, IEquipment
{
    [Export] private PackedScene _projectileScene;

    void IEquipment.PrimaryPress()
    {
        var mousePosition = GetMouseGlobalPosition();
        var projectileSpawnPosition = GetNode<Marker2D>("ProjectileSpawnPoint").GlobalPosition;

        // Instantiate Projectile
        var projectile = _projectileScene.Instantiate<Projectile>();
        {
            projectile.Instigator = GetNode<IEntity>("../");
            projectile.InitialSpeed = 500f;
            projectile.Acceleration = 1000f;
            projectile.Direction = (mousePosition - projectileSpawnPosition).Normalized();
        }

        // Attach Root
        GetTree().Root.AddChild(projectile);
        projectile.GlobalPosition = projectileSpawnPosition;
    }

    void IEquipment.PrimaryRelease()
    {
        GD.Print("Wand released");
    }

    private Vector2 GetMouseGlobalPosition()
    {
        var mouse = GetNode<Mouse>("../MainCamera/Mouse");
        return mouse.GlobalPosition;
    }

    public override void _Process(double delta)
    {
        // Rotate this to face mouse
        var mousePosition = GetMouseGlobalPosition();
        var direction = (mousePosition - GlobalPosition).Normalized();
        Rotation = direction.Angle();
    }
}