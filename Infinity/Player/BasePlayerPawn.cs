using System;
using fms.Effect;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public partial class BasePlayerPawn : CharacterBody2D, IEntity
{
    [ExportGroup("Base Stats")]
    [Export(PropertyHint.Range, "0,1000,1")]
    private float _health = 100f;

    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _moveSpeed = 100f;

    [ExportGroup("Camera Settings")]
    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private PlayerState? _state;

    /// <summary>
    /// </summary>
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
            .Subscribe(this, (x, self) => { self.GetNode<PhysicsBody2DPawn>("PhysicsBody2DPawn").MoveSpeed = x; })
            .AddTo(this);

        // Initialize healthbar
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

    public void Heal(uint amount)
    {
        State.AddEffect(new HealEffect { Value = amount });
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

    void IEntity.AddEffect(string effectName)
    {
        throw new NotImplementedException();
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

        State.AddEffect(new PhysicalDamageEffect { Value = amount });
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
}