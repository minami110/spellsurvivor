using Godot;

public partial class SettingsHud : CanvasLayer
{
    private static SettingsHud? _instance;

    /// <summary>
    /// </summary>
    public static bool IsOpen => _instance is not null && _instance.Visible;

    public override void _EnterTree()
    {
        _instance = this;
        HideHud();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("open_pose"))
        {
            // 現在 タイトルシーンにいる場合は表示しない
            HideHud();
        }
    }

    public override void _ExitTree()
    {
        _instance = null;
    }

    /// <summary>
    /// </summary>
    public static void HideHud()
    {
        if (_instance is null)
        {
            return;
        }

        _instance.Hide();
        _instance.SetProcessInput(false);
    }

    /// <summary>
    /// </summary>
    public static void ShowHud()
    {
        if (_instance is null)
        {
            return;
        }

        _instance.Show();
        _instance.SetProcessInput(true);
    }
}