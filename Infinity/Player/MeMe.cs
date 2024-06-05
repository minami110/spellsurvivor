using Godot;
using R3;

namespace fms;

public partial class MeMe : CharacterBody2D, IPawn
{
    [Export]
    private float _moveSpeed = 100f;

    private Vector2 _nextMoveDirection;

    private PlayerState? _playerState;

    public override void _PhysicsProcess(double delta)
    {
        if (!(_nextMoveDirection.LengthSquared() > 0f))
        {
            return;
        }

        // Update PlayerForward
        var angle = Mathf.Atan2(_nextMoveDirection.Y, _nextMoveDirection.X);
        Rotation = angle;

        var motion = _nextMoveDirection * (float)delta * _moveSpeed;
        MoveAndCollide(motion);
    }

    public void SetPlayerState(PlayerState state)
    {
        _playerState = state;
        _playerState.MoveSpeed.Subscribe(this, (x, state) => state._moveSpeed = x).AddTo(this);
    }

    public void TakeDamage(float amount)
    {
        if (_playerState is null)
        {
            return;
        }

        var effect = new PhysicalDamageEffect
        {
            Value = amount
        };

        _playerState.AddEffect(effect);
        _playerState.SolveEffect();

        // 統計をおくる
        StaticsManager.CommitDamage(StaticsManager.DamageTakeOwner.Player, amount, GlobalPosition);
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