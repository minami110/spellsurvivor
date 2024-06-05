using Godot;
using R3;

namespace fms;

public sealed class GameConfig
{
    private const string _SECTION_GLOBAL = "Global";
    private const string _CONFIG_PATH = "user://config.cfg";

    public static readonly GameConfig Singleton;

    // Audio
    public readonly ReactiveProperty<float> AudioMasterVolume = new(0.8f);

    // General
    public readonly ReactiveProperty<string> Locale = new();

    // Graphics
    public readonly ReactiveProperty<bool> ShowDamageNumbers = new(true);

    static GameConfig()
    {
        Singleton = new GameConfig();
        Singleton.LoadConfig();
    }

    public void SaveConfig()
    {
        // Save current config
        var config = new ConfigFile();

        // Globals
        config.SetValue(_SECTION_GLOBAL, nameof(AudioMasterVolume), AudioMasterVolume.Value);
        config.SetValue(_SECTION_GLOBAL, nameof(Locale), Locale.Value);
        config.SetValue(_SECTION_GLOBAL, nameof(ShowDamageNumbers), ShowDamageNumbers.Value);
        config.Save(_CONFIG_PATH);
    }

    private void LoadConfig()
    {
        // Load config
        var config = new ConfigFile();
        var error = config.Load(_CONFIG_PATH);
        if (error == Error.Ok)
        {
            GD.Print("[GameConfig] Loaded config file");

            // General
            AudioMasterVolume.Value = (float)config.GetValue(_SECTION_GLOBAL, nameof(AudioMasterVolume), 0.8f);

            // Audio
            Locale.Value = (string)config.GetValue(_SECTION_GLOBAL, nameof(Locale), string.Empty);

            // Graphics
            ShowDamageNumbers.Value = (bool)config.GetValue(_SECTION_GLOBAL, nameof(ShowDamageNumbers), true);
        }
        else
        {
            GD.PrintErr($"[GameConfig] Failed to load config file: {_CONFIG_PATH}");
            SaveConfig();
            GD.Print("[GameConfig] Created new config file");
        }
    }
}