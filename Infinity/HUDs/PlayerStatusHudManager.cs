using System;
using System.Globalization;
using Godot;
using R3;

namespace fms;

public partial class PlayerStatusHudManager : Node
{
    [Export]
    private PlayerStatusRaw _money = null!;

    [Export]
    private PlayerStatusRaw _maxHealth = null!;

    [Export]
    private PlayerStatusRaw _moveSpeed = null!;

    [Export]
    private PlayerStatusRaw _dodgeRate = null!;

    public override void _Ready()
    {
        var n = GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        if (n is not EntityState playerState)
        {
            throw new ApplicationException("PlayerState is not found");
        }

        // Money
        playerState.Money.ChangedCurrentValue.Subscribe(this, (v, state) => { state._money.Value = v.ToString(); })
            .AddTo(this);

        // MaxHealth
        playerState.Health.ChangedMaxValue.Subscribe(this, (v, state) => { state._maxHealth.Value = v.ToString(); })
            .AddTo(this);
        _maxHealth.DefaultValue = playerState.Health.DefaultMaxValue.ToString();

        // MoveSpeed
        playerState.MoveSpeed.ChangedCurrentValue.Subscribe(this,
            (v, state) => { state._moveSpeed.Value = v.ToString(CultureInfo.InvariantCulture); }).AddTo(this);
        _moveSpeed.DefaultValue = playerState.MoveSpeed.DefaultValue.ToString(CultureInfo.InvariantCulture);

        // DodgeRate
        playerState.DodgeRate.ChangedCurrentValue.Subscribe(this, (v, state) =>
        {
            // % 表記にするために 100 倍する
            state._dodgeRate.Value = (v * 100f).ToString(CultureInfo.InvariantCulture);
        }).AddTo(this);
        _dodgeRate.DefaultValue = playerState.DodgeRate.CurrentValue.ToString(CultureInfo.InvariantCulture);
    }
}