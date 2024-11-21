using System;
using Godot;

namespace fms;

public partial class PhysicsBody2DPawn : Node, IPawn
{
    private PhysicsBody2D? _body;
    private Vector2 _next;
    private Vector2 _prev;

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
        _prev = _next;
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

    Vector2 IPawn.GetAimDirection()
    {
        // 現在狙っている方向を返す
        // コントローラーがつながっている場合は, 右スティックの入力方法を返す
        if (Input.GetConnectedJoypads().Count > 0)
        {
            var deadZone = 0.2f;
            // ToDo: InputAction 使用したほうがいいかも
            var joyX = Input.GetJoyAxis(0, JoyAxis.RightX);
            if (joyX < deadZone && joyX > -deadZone)
            {
                joyX = 0;
            }

            var joyY = Input.GetJoyAxis(0, JoyAxis.RightY);
            if (joyY < deadZone && joyY > -deadZone)
            {
                joyY = 0;
            }

            var joy = (Vector2.Right * joyX + Vector2.Down * joyY).Normalized();

            if (joy.LengthSquared() > 0)
            {
                return joy;
            }
        }

        // ToDo: ↓ の処理は実装先で柔軟にやってもらったほうが硬い
        // つながっていない場合, スティックを倒していない場合は, 最後に動いた方向を返す
        return _prev.Normalized();
    }

    void IPawn.MoveForward(in Vector2 dir)
    {
        _next = dir;
    }
}