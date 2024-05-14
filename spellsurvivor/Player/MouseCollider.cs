using Godot;
using System;
namespace spellsurvivor;

public partial class MouseCollider : Area2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is IEntity)
        {
            GD.Print($"Collide with {area.Name}");
        }
    }
}