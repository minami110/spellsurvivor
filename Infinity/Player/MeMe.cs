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
    private Vector2 NextMoveDirection { get; set; }

    /// <summary>
    /// 停止する直前まで移動していた方向
    /// </summary>
    public Vector2 LatestMoveDirection { get; private set; } = Vector2.Right;

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

        if (NextMoveDirection.LengthSquared() <= 0f)
        {
            controller.SendSignalStop();
            return;
        }


        LatestMoveDirection = NextMoveDirection;
        var motion = LatestMoveDirection * (float)delta * _moveSpeed;
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

    public void Heal(uint amount)
    {
        AddEffect(new HealEffect { Value = amount });
        var info = new DamageReport
        {
            Amount = -amount,
            Victim = this,
            Instigator = this, // 自分自身で回復ということに..
            Position = GlobalPosition,
            IsDead = false
        };
        StaticsManager.CommitDamage(info);
    }

    public void TakeDamage(float amount, Node instigator)
    {
        AddEffect(new PhysicalDamageEffect { Value = amount });
        var info = new DamageReport
        {
            Amount = amount,
            Victim = this,
            Instigator = instigator,
            Position = GlobalPosition,
            IsDead = false
        };
        StaticsManager.CommitDamage(info);
    }

    private void AddEffect(EffectBase effect)
    {
        GetNode<PlayerState>("%PlayerState").AddEffect(effect);
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
        NextMoveDirection = dir;
    }
}