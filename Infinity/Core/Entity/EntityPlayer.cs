using System;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public partial class EntityPlayer : CharacterBody2D, IEntity
{
    [ExportGroup("Base Status")]
    [Export(PropertyHint.Range, "0,100,")]
    private uint _money = 10u;

    [Export(PropertyHint.Range, "0,1000,1")]
    private uint _health = 100u;

    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private uint _moveSpeed = 100u;

    [Export(PropertyHint.Range, "0,100,0.1,suffix:%")]
    private float _dodgeRate;

    [ExportGroup("Camera Settings")]
    [Export]
    private Vector2I _cameraLimit = new(550, 550);

    public override void _EnterTree()
    {
        // Crate PlayerState
        State = new EntityState(
            _money,
            _health,
            _moveSpeed,
            _dodgeRate * 0.01f
        );
        AddChild(State);
        State.AddToGroup(GroupNames.PlayerState);

        // Join to Player Group
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
            CauserPath = "Player",
            Position = GlobalPosition,
            IsVictimDead = false
        };
        StaticsManager.ReportDamage(info);
    }

    public EntityState State { get; private set; } = null!;

    void IEntity.ApplayDamage(float amount, IEntity instigator, Node causer, string causerPath)
    {
        if (amount == 0f)
        {
            return;
        }

        // DodgeRate による回避判定
        var dodgeRate = State.DodgeRate.CurrentValue;
        switch (dodgeRate)
        {
            case >= 1f: // 確定で回避に成功する
                StaticsManager.SuccessDodge(GlobalPosition);
                return;
            case > 0f:
                var chance = (float)GD.RandRange(0f, 1f);
                if (dodgeRate > chance)
                {
                    StaticsManager.SuccessDodge(GlobalPosition);
                    return;
                }

                break;
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
            CauserPath = causerPath,
            Position = GlobalPosition,
            IsVictimDead = false
        };
        StaticsManager.ReportDamage(info);
    }

    bool IEntity.IsDead => throw new NotImplementedException();

    Vector2 IEntity.Position => GlobalPosition;

    void IGodotNode.AddChild(Node node)
    {
        AddChild(node);
    }

    void IGodotNode.RemoveChild(Node node)
    {
        RemoveChild(node);
    }
}
