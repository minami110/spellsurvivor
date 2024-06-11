using Godot;
using R3;

namespace fms;

public partial class MeMe : CharacterBody2D, IPawn
{
    [Export]
    private float _moveSpeed = 100f;

    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private Vector2 _nextMoveDirection;

    public override void _Ready()
    {
        var playerState = GetNode<PlayerState>("%PlayerState");
        playerState.AddEffect(new AddMoveSpeedEffect { Value = _moveSpeed });
        playerState.MoveSpeed
            .Subscribe(this, (x, state) => { state._moveSpeed = x; })
            .AddTo(this);

        var camera = GetNode<Camera2D>("%MainCamera");
        camera.LimitLeft = -_cameraLimit.X;
        camera.LimitRight = _cameraLimit.X;
        camera.LimitTop = -_cameraLimit.Y;
        camera.LimitBottom = _cameraLimit.Y;
    }

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

    public void TakeDamage(float amount)
    {
        var effect = new PhysicalDamageEffect
        {
            Value = amount
        };

        GetNode<PlayerState>("%PlayerState").AddEffect(effect);

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