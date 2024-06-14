using Godot;
using R3;

namespace fms;

public partial class MeMe : CharacterBody2D, IPawn
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _health = 100f;

    [Export(PropertyHint.Range, "0,1000,1")]
    private float _moveSpeed = 100f;

    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private Vector2 _nextMoveDirection;

    public override void _EnterTree()
    {
        AddToGroup(Constant.GroupNamePlayer);
    }

    public override void _Ready()
    {
        var playerState = GetNode<PlayerState>("%PlayerState");
        playerState.AddEffect(new AddMoveSpeedEffect { Value = _moveSpeed });
        playerState.MoveSpeed
            .Subscribe(this, (x, state) => { state._moveSpeed = x; })
            .AddTo(this);


        playerState.AddEffect(new AddHealthEffect { Value = _health });
        playerState.AddEffect(new AddMaxHealthEffect { Value = _health });

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

    public void AddEffect(EffectBase effect)
    {
        GetNode<PlayerState>("%PlayerState").AddEffect(effect);

        // 統計をおくる
        switch (effect)
        {
            case PhysicalDamageEffect damageEffect:
                StaticsManager.CommitDamage(StaticsManager.DamageTakeOwner.Player, damageEffect.Value, GlobalPosition);
                break;
            case HealEffect healEffect:
                StaticsManager.CommitDamage(StaticsManager.DamageTakeOwner.Player, -healEffect.Value, GlobalPosition);
                break;
        }
    }

    public void TakeDamage(float amount)
    {
        AddEffect(new PhysicalDamageEffect { Value = amount });
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