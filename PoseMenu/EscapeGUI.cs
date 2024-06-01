using Godot;

namespace fms;

public partial class EscapeGUI : CanvasLayer
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _titleScene = null!;

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
        // タイトル画面に遷移する
        SceneManager.GoToScene(_titleScene);
    }

    private void HideGui()
    {
        Hide();
        GetTree().Paused = false;
    }
}