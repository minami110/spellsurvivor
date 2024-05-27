using Godot;

namespace fms;

public partial class Title : Node
{
    [Export]
    private Label _appNameLabel = null!;

    [Export]
    private Label _appVersionLabel = null!;

    [Export]
    private Label _godotVersionLabel = null!;

    [Export(PropertyHint.File, "*.tscn")]
    private string _mainGameScene = string.Empty;


    public async void OnStartButtonPressed()
    {
        GetNode<SceneManager>("/root/SceneManager").GoToScene(_mainGameScene);
    }

    public void OnExitButtonPressed()
    {
        // Close Application
        GetTree().Quit();
    }

    public override void _Ready()
    {
        // Update App Name Label
        {
            var appName = ProjectSettings.GetSetting("application/config/name");
            _appNameLabel.Text = appName.ToString();
        }

        // Update version label
        {
            // Get Application version info
            var appVersion = ProjectSettings.GetSetting("application/config/version");
            _appVersionLabel.Text = $"{appVersion}";

            // Get GodotEngine version info
            var godotVersionInfo = Engine.GetVersionInfo();
            _godotVersionLabel.Text =
                $"Powered by Godot ({godotVersionInfo["major"]}.{godotVersionInfo["minor"]}.{godotVersionInfo["patch"]} {godotVersionInfo["status"]})";
        }
    }
}