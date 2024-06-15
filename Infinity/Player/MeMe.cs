using Godot;
using R3;

namespace fms;

public enum PawnFaceDirection
{
    Right,
    Left
}

public partial class MeMe : CharacterBody2D, IPawn
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _health = 100f;

    [Export(PropertyHint.Range, "0,1000,1")]
    private float _moveSpeed = 100f;

    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private readonly ReactiveProperty<PawnFaceDirection> _faceDirection = new(PawnFaceDirection.Right);
    public ReadOnlyReactiveProperty<PawnFaceDirection> FaceDirection => _faceDirection;

    /// <summary>
    /// </summary>
    public Vector2 MoveDirection { get; private set; }

    public override void _EnterTree()
    {
        AddToGroup(Constant.GroupNamePlayer);
    }

    public override void _Ready()
    {
        // Inititialize PlayerState
        var playerState = GetNode<PlayerState>("%PlayerState");
        playerState.AddEffect(new AddMoveSpeedEffect { Value = _moveSpeed });
        playerState.AddEffect(new AddHealthEffect { Value = _health });
        playerState.AddEffect(new AddMaxHealthEffect { Value = _health });

        // Initialize Camera
        var camera = GetNode<Camera2D>("%MainCamera");
        camera.LimitLeft = -_cameraLimit.X;
        camera.LimitRight = _cameraLimit.X;
        camera.LimitTop = -_cameraLimit.Y;
        camera.LimitBottom = _cameraLimit.Y;

        // Subscribes
        playerState.MoveSpeed
            .Subscribe(this, (x, self) => { self._moveSpeed = x; })
            .AddTo(this);
        _faceDirection.AddTo(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        var controller = GetNode<PlayerAnimationController>("AnimationController");

        if (!(MoveDirection.LengthSquared() > 0f))
        {
            controller.SendSignalStop();
            return;
        }

        var motion = MoveDirection * (float)delta * _moveSpeed;
        MoveAndCollide(motion);

        // Update Animation
        controller.SendSignelMove();
        if (motion.X > 0)
        {
            _faceDirection.Value = PawnFaceDirection.Right;
            controller.SendSignalMoveRight();
        }
        else if (motion.X < 0)
        {
            _faceDirection.Value = PawnFaceDirection.Left;
            controller.SendSignalMoveLeft();
        }
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
        MoveDirection = dir;
    }
}