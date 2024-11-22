using System;
using Godot;

namespace fms;

public partial class PhysicsBody2DPawn : Node, IPawn
{
    private PhysicsBody2D? _body;
    private Vector2 _next;

    public float MoveSpeed { get; set; } = 100f;

    private PhysicsBody2D Body
    {
        get
        {
            if (_body is null)
            {
                var body = GetParentOrNull<PhysicsBody2D>();
                _body = body ?? throw new ApplicationException("Failed to get CharacterBody2D in parent");
            }

            return _body;
        }
    }

    public override void _EnterTree()
    {
        AddToGroup(GroupNames.Pawn);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_next.LengthSquared() <= 0f)
        {
            return;
        }

        var motion = _next * (float)delta * MoveSpeed;
        Body.MoveAndCollide(motion);
        _next = Vector2.Zero;
    }

    void IPawn.PrimaryPressed()
    {
        // Do nothing
    }

    void IPawn.PrimaryReleased()
    {
        // Do nothing
    }

    void IPawn.MoveForward(in Vector2 dir)
    {
        _next = dir;
    }
}