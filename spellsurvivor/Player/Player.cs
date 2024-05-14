using System;
using Godot;

namespace spellsurvivor;

public partial class Player : Area2D, IEntity
{
    private Vector2 _screenSize; // Size of the game window.

    [Export]
    public float Health { get; private set; } = 100f;

    [Export] 
    public float MaxHealth { get; private set; } = 100f;

    [Export]
    public float Speed { get; private set; } = 400;

    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
        
        // Update Health bar
        var healthBar = GetNode<ProgressBar>("HealthBar");
        healthBar.MaxValue = MaxHealth;
        healthBar.Value = Health;
    }
    
    void IEntity.TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            // Emit signal to main scene
            throw new NotImplementedException();
        }
        
        // Update Health bar
        var healthBar = GetNode<ProgressBar>("HealthBar");
        healthBar.Value = Health;
    }

    public override void _Process(double delta)
    {
        var velocity = Vector2.Zero; // The player's movement vector.

        if (Input.IsActionPressed("move_right"))
        {
            velocity.X += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            velocity.X -= 1;
        }

        if (Input.IsActionPressed("move_down"))
        {
            velocity.Y += 1;
        }

        if (Input.IsActionPressed("move_up"))
        {
            velocity.Y -= 1;
        }

        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * Speed;
            Position += velocity * (float)delta;
        }
    }


}