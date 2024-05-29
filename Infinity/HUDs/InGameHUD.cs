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

    [Export]
    private Control _equipmentContainer = null!;

    [Export]
    private PackedScene _equipmentPackedScene = null!;

    public override void _Ready()
    {
        var gm = Main.GameMode;
        var ps = Main.PlayerState;

        // Subscribe player health
        var d1 = ps.Health.Subscribe(OnHealthChanged);
        var d2 = ps.MaxHealth.Subscribe(OnHealthChanged);

        // Subscribe wave info
        var d3 = gm.Wave.Subscribe(x => _currentWaveLabel.Text = $"Wave {x}");
        var d4 = gm.RemainingWaveSecond.Subscribe(x => _waveTimerLabel.Text = $"{x:000}");
        var d5 = gm.WaveStarted.Subscribe(_ => OnWaveStarted());
        var d6 = gm.WaveEnded.Subscribe(_ => OnWaveEnded());

        // Add disposables when this node is exited tree
        Disposable.Combine(d1, d2, d3, d4, d5, d6).AddTo(this);
    }

    private void OnHealthChanged(float _)
    {
        var playerState = Main.PlayerState;

        // Update health bar
        _healthBar.MinValue = 0f;
        _healthBar.MaxValue = playerState.MaxHealth.CurrentValue;
        _healthBar.Value = playerState.Health.CurrentValue;

        // Update health text
        _healthText.Text = $"{playerState.Health.CurrentValue} / {playerState.MaxHealth.CurrentValue}";
    }

    private void OnWaveEnded()
    {
        foreach (var child in _equipmentContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void OnWaveStarted()
    {
        // Spawn equipments
        foreach (var equipment in Main.GameMode.Equipments)
        {
            var node = _equipmentPackedScene.Instantiate<InGameEquipment>();
            {
                node.ItemSettings = equipment;
            }
            _equipmentContainer.AddChild(node);
        }
    }
}