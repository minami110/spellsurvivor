using Godot;
using R3;

namespace fms;

public sealed class GameConfig
{
    private const string _SECTION_GLOBAL = "Global";
    private const string _CONFIG_PATH = "user://config.cfg";

    public static readonly GameConfig Singleton;
    public readonly ReactiveProperty<float> AudioMasterVolume = new(0.8f);
    public readonly ReactiveProperty<string> Locale = new();

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
        config.SetValue(_SECTION_GLOBAL, "AudioMasterVolume", AudioMasterVolume.Value);
        config.SetValue(_SECTION_GLOBAL, "Locale", Locale.Value);
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

            // Globals
            AudioMasterVolume.Value = (float)config.GetValue(_SECTION_GLOBAL, "AudioMasterVolume", 0.8f);
            Locale.Value = (string)config.GetValue(_SECTION_GLOBAL, "Locale", string.Empty);
        }
        else
        {
            GD.PrintErr($"[GameConfig] Failed to load config file: {_CONFIG_PATH}");
            SaveConfig();
            GD.Print("[GameConfig] Created new config file");
        }
    }
}