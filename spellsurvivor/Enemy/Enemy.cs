#nullable enable
using System.Collections.Generic;
using Godot;

namespace spellsurvivor;

public partial class Enemy : RigidBody2D, IEntity
{
    [Export(PropertyHint.Range, "0,100,1")]
    public float MaxHealth { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0,100,1")]
    public float Health { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0,1000,1")] public float MoveSpeed { get;  set; } = 50f;

    public Race Race => Race.Slime;

    void IEntity.TakeDamage(float amount, IEntity? instigator)
    {
        if (instigator is not null)
        {
            if (instigator.Race != Race.Player)
            {
                return;
            }
        }

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            QueueFree();
        }
        else
        {
            UpdateHealthBar();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        var healthBar = GetNode<ProgressBar>("HealthBar");
        healthBar.MaxValue = MaxHealth;
        healthBar.SetValueNoSignal(Health);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var playerPosition = Main.GetPlayerGlobalPosition();
        
        // Move to player
        var direction = playerPosition - GlobalPosition;
        direction = direction.Normalized();
        var force =  direction * MoveSpeed;
        ApplyForce(force);
    }
}