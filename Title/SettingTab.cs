using Godot;

namespace fms;

public partial class SettingTab : TabContainer
{
    [Export]
    private Slider _masterVolumeSlider = null!;

    public override void _Ready()
    {
        var config = GameConfig.Singleton;

        // Master Volume Slide initialization and bindings
        _masterVolumeSlider.MinValue = 0;
        _masterVolumeSlider.MaxValue = 1;
        _masterVolumeSlider.Step = 0.05f;
        _masterVolumeSlider.Value = config.AudioMasterVolume.Value;
        _masterVolumeSlider.ValueChanged += value => { config.AudioMasterVolume.Value = (float)value; };
    }

    public override void _ExitTree()
    {
        // Save config
        var config = GameConfig.Singleton;
        config.SaveConfig();
    }
}