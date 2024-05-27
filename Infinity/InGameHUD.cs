using System;
using Godot;
using R3;

namespace fms;

public sealed partial class InGameHUD : CanvasLayer
{
    [Export]
    private TextureProgressBar _healthBar = null!;

    [Export]
    private Label _healthText = null!;

    private IDisposable _waveStartSubscription = null!;

    public override void _Ready()
    {
        var playerState = Main.GameMode.GetPlayerState();
        var d1 = playerState.Health.Subscribe(OnHealthChanged);
        var d2 = playerState.MaxHealth.Subscribe(OnHealthChanged);
        _waveStartSubscription = Disposable.Combine(d1, d2);
    }

    public override void _ExitTree()
    {
        _waveStartSubscription.Dispose();
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