using fms.Weapon;
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
        var ps = Main.PlayerState;

        // Subscribe player health
        var d1 = ps.Health.Subscribe(OnHealthChanged);
        var d2 = ps.MaxHealth.Subscribe(OnHealthChanged);

        // Subscribe wave info
        var ws = Main.WaveState;
        var d3 = ws.Wave.Subscribe(x => _currentWaveLabel.Text = $"Wave {x}");
        var d4 = ws.BattlePhaseTimeLeft.Subscribe(x => _waveTimerLabel.Text = $"{x:000}");
        var d5 = ws.Phase.Subscribe(p =>
        {
            if (p == WavePhase.Battle)
            {
                OnBattleWaveStarted();
            }
            else
            {
                OnBattleWaveEnded();
            }
        });

        // Add disposables when this node is exited tree
        Disposable.Combine(d1, d2, d3, d4, d5).AddTo(this);
    }

    private void OnBattleWaveEnded()
    {
        // Hide 
        Hide();

        if (_equipmentContainer.GetChildCount() == 0)
        {
            return;
        }

        foreach (var child in _equipmentContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void OnBattleWaveStarted()
    {
        // Spawn equipments
        var nodes = GetTree().GetNodesInGroup(Constant.GroupNameWeapon);
        foreach (var n in nodes)
        {
            if (n is not WeaponBase weapon)
            {
                continue;
            }

            var node = _equipmentPackedScene.Instantiate<InGameEquipment>();
            {
                node.Weapon = weapon;
            }
            _equipmentContainer.AddChild(node);
        }

        // Show
        Show();
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
}