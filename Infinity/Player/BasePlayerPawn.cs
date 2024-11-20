using System;
using fms.Effect;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public enum PawnFaceDirection
{
    Right,
    Left
}

public partial class BasePlayerPawn : CharacterBody2D, IPawn, IEntity
{
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _health = 100f;

    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _moveSpeed = 100f;

    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private readonly ReactiveProperty<PawnFaceDirection> _faceDirection = new(PawnFaceDirection.Right);

    private PlayerState? _state;
    public ReadOnlyReactiveProperty<PawnFaceDirection> FaceDirection => _faceDirection;

    /// <summary>
    /// </summary>
    private Vector2 NextMoveDirection { get; set; }

    /// <summary>
    /// 前フレームの移動方向
    /// </summary>
    private Vector2 PrevLinearVelocity { get; set; } = Vector2.Right;

    private PlayerState State
    {
        get
        {
            if (_state is null)
            {
                var state = GetNode<PlayerState>("%PlayerState");
                _state = state ?? throw new ApplicationException("Failed to get PlayerState in children");
            }

            return _state;
        }
    }

    public override void _EnterTree()
    {
        AddToGroup(Constant.GroupNamePlayer);
    }

    public override void _Ready()
    {
        // Inititialize PlayerState
        State.AddEffect(new Wing { Amount = _moveSpeed });
        State.AddEffect(new AddHealthEffect { Value = _health });
        State.AddEffect(new AddMaxHealthEffect { Value = _health });

        // Initialize Camera
        var camera = GetNode<Camera2D>("%MainCamera");
        camera.LimitLeft = -_cameraLimit.X;
        camera.LimitRight = _cameraLimit.X;
        camera.LimitTop = -_cameraLimit.Y;
        camera.LimitBottom = _cameraLimit.Y;

        // Subscribes
        State.MoveSpeed
            .Subscribe(this, (x, self) => { self._moveSpeed = x; })
            .AddTo(this);
        _faceDirection.AddTo(this);

        var healthBar = GetNodeOrNull<Range>("HealthBar");
        if (healthBar is not null)
        {
            State.MaxHealth
                .Subscribe(healthBar, (x, s) =>
                {
                    s.MaxValue = x;
                    if (s.Value < s.MaxValue)
                    {
                        s.Show();
                    }
                    else
                    {
                        s.Hide();
                    }
                })
                .AddTo(healthBar);
            State.Health
                .Subscribe(healthBar, (x, s) =>
                {
                    s.SetValueNoSignal(x);
                    if (s.Value < s.MaxValue)
                    {
                        s.Show();
                    }
                    else
                    {
                        s.Hide();
                    }
                })
                .AddTo(healthBar);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var controller = GetNode<BasePlayerAnimator>("%Animator");

        if (NextMoveDirection.LengthSquared() <= 0f)
        {
            controller.SendSignalStop();
            return;
        }

        PrevLinearVelocity = NextMoveDirection;
        var motion = PrevLinearVelocity * (float)delta * _moveSpeed;
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
            Causer = this,
            Position = GlobalPosition,
            IsDead = false
        };
        StaticsManager.CommitDamage(info);
    }

    private void AddEffect(EffectBase effect)
    {
        State.AddEffect(effect);
    }

    void IEntity.ApplayDamage(float amount, IEntity instigator, Node causer)
    {
        // Dodge がある場合は, ここで回避する
        var dodgeRate = State.DodgeRate.CurrentValue;
        if (dodgeRate > 0f)
        {
            var chance = (float)GD.RandRange(0f, 1f);
            if (dodgeRate > chance)
            {
                // 回避成功なのでダメージ処理を行わず, 回避演出を行う
                StaticsManager.SuccessDodge(GlobalPosition);
                return;
            }
        }

        AddEffect(new PhysicalDamageEffect { Value = amount });
        var info = new DamageReport
        {
            Amount = amount,
            Victim = this,
            Instigator = instigator,
            Causer = causer,
            Position = GlobalPosition,
            IsDead = false
        };
        StaticsManager.CommitDamage(info);
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

        // つながっていない場合, スティックを倒していない場合は, 最後に動いた方向を返す
        return PrevLinearVelocity.Normalized();
    }

    void IPawn.MoveForward(in Vector2 dir)
    {
        NextMoveDirection = dir;
    }
}