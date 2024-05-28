using Godot;

namespace fms;

public partial class MeMe : CharacterBody2D, IPawn
{
    private Vector2 _nextMoveDirection;


    public override void _Process(double delta)
    {
        if (!(_nextMoveDirection.LengthSquared() > 0f))
        {
            return;
        }

        // Update PlayerForward
        var angle = Mathf.Atan2(_nextMoveDirection.Y, _nextMoveDirection.X);
        Rotation = angle;

        // Update Position
        var speed = Main.PlayerState.MoveSpeed.CurrentValue;

        var motion = _nextMoveDirection * (float)delta * speed;
        MoveAndCollide(motion);
    }

    public void TakeDamage(float amount, IEntity? instigator)
    {
        if (instigator is null)
        {
            return;
        }

        var state = Main.PlayerState;

        // ToDo: Create Physical Damage Effect
        var effect = new PhysicalDamageEffect
        {
            Value = amount
        };

        state.AddEffect(effect);
        state.SolveEffect();
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
        _nextMoveDirection = dir;
    }
}