using Godot;

public partial class EscapeGUI : Node2D
{
    public void OnExitPressed()
    {
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