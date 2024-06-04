using Godot;
using R3;

namespace fms;

public partial class EscapeGUI : CanvasLayer
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _titleScene = null!;

    [Export]
    private Button _settingsbutton = null!;

    [Export]
    private Control _settingsRoot = null!;

    public override void _Ready()
    {
        _settingsRoot.Hide();

        _settingsbutton.PressedAsObservable().Subscribe(_ =>
        {
            if (_settingsRoot.Visible)
            {
                return;
            }

            _settingsRoot.Show();
        }).AddTo(this);
        Hide();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("open_pose"))
        {
            if (_settingsRoot.Visible)
            {
                _settingsRoot.Hide();
                return;
            }

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