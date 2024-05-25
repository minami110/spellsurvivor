using Godot;

public partial class EscapeGUI : CanvasLayer
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _titleScene = null!;

    public void OnExitPressed()
    {
        if (!Visible)
        {
            return;
        }

        // Exit application
        GetTree().Quit();
    }

    public void OnResumePressed()
    {
        if (!Visible)
        {
            return;
        }

        HideGui();
    }

    public void OnTitlePressed()
    {
        if (!Visible)
        {
            return;
        }

        // ポーズ解除する
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile(_titleScene);
    }

    public override void _Ready()
    {
        Hide();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("open_pose"))
        {
            if (Visible)
            {
                HideGui();
            }
            else
            {
                // Pause Game
                GetTree().Paused = true;
                Show();
            }
        }
    }

    private void HideGui()
    {
        Hide();
        GetTree().Paused = false;
    }
}