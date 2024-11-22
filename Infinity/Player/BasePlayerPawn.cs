using System;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public partial class BasePlayerPawn : CharacterBody2D, IEntity
{
    [ExportGroup("Base Status")]
    [Export(PropertyHint.Range, "0,1000,1")]
    private uint _health = 100u;

    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private uint _moveSpeed = 100u;

    [Export(PropertyHint.Range, "0,1,0.01,suffix:%")]
    private float _dodgeRate;

    [ExportGroup("Camera Settings")]
    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    private PlayerState State { get; set; } = null!;

    public override void _EnterTree()
    {
        // Crate PlayerState
        State = new PlayerState(
            _health,
            _moveSpeed,
            _dodgeRate
        );
        AddChild(State);
        AddToGroup(GroupNames.Player);
    }

    public override void _Ready()
    {
        // Initialize Camera
        var camera = GetNode<Camera2D>("%MainCamera");
        camera.LimitLeft = -_cameraLimit.X;
        camera.LimitRight = _cameraLimit.X;
        camera.LimitTop = -_cameraLimit.Y;
        camera.LimitBottom = _cameraLimit.Y;

        // Subscribes
        var pawn = GetNode<PhysicsBody2DPawn>("PhysicsBody2DPawn");
        State.MoveSpeed.ChangedCurrentValue
            .Subscribe(pawn, (x, state) => { state.MoveSpeed = x; })
            .AddTo(pawn);

        // Initialize healthbar
        var healthBar = GetNodeOrNull<Range>("HealthBar");
        if (healthBar is not null)
        {
            State.Health.ChangedMaxValue
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
            State.Health.ChangedCurrentValue
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
        State.Heal(amount);
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
        if (amount == 0f)
        {
            return;
        }

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

        // ダメージ処理
        State.ApplyDamage((uint)amount);

        // 統計情報を送信
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