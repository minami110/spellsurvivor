using System;
using Godot;
using R3;

namespace spellsurvivor;

public sealed partial class InGameHUD : CanvasLayer
{
    [Export]
    private TextureProgressBar _healthBar = null!;

    [Export]
    private Label _healthText = null!;

    private IDisposable _waveStartSubscription;

    public override void _Ready()
    {
        var playerState = Main.GameMode.GetPlayerState();
        playerState.Health.Subscribe(OnHealthChanged);
        playerState.MaxHealth.Subscribe(OnHealthChanged);
    }

    private void OnHealthChanged(float _)
    {
        var playerState = Main.GameMode.GetPlayerState();
        _healthBar.MinValue = 0f;
        _healthBar.MaxValue = playerState.MaxHealth.CurrentValue;

        _healthBar.Value = playerState.Health.CurrentValue;

        _healthText.Text = $"{playerState.Health.CurrentValue} / {playerState.MaxHealth.CurrentValue}";
    }
}