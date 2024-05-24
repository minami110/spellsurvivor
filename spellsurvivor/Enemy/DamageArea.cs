using Godot;

namespace fms;

public partial class DamageArea : Area2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is IEntity entity)
        {
            entity.TakeDamage(10, GetNode<Enemy>(".."));
        }
    }
}