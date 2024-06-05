using System;
using Godot;
using R3;

namespace fms.Projectile;

public partial class RotateDisk : ProjectileBase
{
    [Export]
    private RigidBody2D _rigidBody = null!;

    [Export]
    private Area2D _enemyDamageArea = null!;
    
    public float Radius { get; set; }

    public float SecondPerRound { get; set; } = 1f;

    public float Angle { get; set; } = 0f;
    
    private float _angularVelocity = 0f;
        
    public override void _Ready()
    {
        // Set rigidbody parameter
        _rigidBody.GlobalPosition = new Vector2(0f, 0f);
        _rigidBody.RotationDegrees = Angle;
        
        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
        
        // Calculate Angular Velocity
        _angularVelocity = Mathf.DegToRad(360 / SecondPerRound);

    }

    private double _timer = 0;
    public override void _Process(double delta)
    {
        _timer += delta;

        var PositionX = (float)Mathf.Cos(_timer * _angularVelocity);
        var PositionY = (float)Mathf.Sin(_timer * _angularVelocity);
        
        var unitVec = new Vector2(PositionX, PositionY);
        
        _rigidBody.GlobalPosition = (unitVec * Radius) + Main.PlayerNode.GlobalPosition;
    }
        
        
    
    private void OnEnemyBodyEntered(Enemy enemy)
    {
        enemy.TakeDamage(BaseDamage);
        KillThis();
    }
}