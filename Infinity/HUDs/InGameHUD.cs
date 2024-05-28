using System;
using Godot;
using R3;

namespace fms;

public sealed partial class InGameHUD : CanvasLayer
{
    [ExportSubgroup("Internal References")]
    [Export]
    private TextureProgressBar _healthBar = null!;

    [Export]
    private Label _healthText = null!;

    [Export]
    private Label _currentWaveLabel = null!;

    [Export]
    private Label _waveTimerLabel = null!;

    private IDisposable _waveStartSubscription = null!;

    public override void _Ready()
    {
        // Subscribe player health
        var playerState = Main.GameMode.GetPlayerState();
        var d1 = playerState.Health.Subscribe(OnHealthChanged);
        var d2 = playerState.MaxHealth.Subscribe(OnHealthChanged);

        // Subscribe wave info
        var d3 = Main.GameMode.Wave.Subscribe(x => _currentWaveLabel.Text = $"Wave {x}");

        _waveStartSubscription = Disposable.Combine(d1, d2, d3);
    }

    public override void _ExitTree()
    {
        _waveStartSubscription.Dispose();
    }

    private void OnHealthChanged(float _)
    {
        var playerState = Main.GameMode.GetPlayerState();

        // Update health bar
        _healthBar.MinValue = 0f;
        _healthBar.MaxValue = playerState.MaxHealth.CurrentValue;
        _healthBar.Value = playerState.Health.CurrentValue;

        // Update health text
        _healthText.Text = $"{playerState.Health.CurrentValue} / {playerState.MaxHealth.CurrentValue}";
    }
}