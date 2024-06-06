using Godot;
using R3;

namespace fms;

/// <summary>
///     ポーズ画面の Hud (Singleton)
/// </summary>
public partial class PauseHud : CanvasLayer
{
    [ExportGroup("Internal References")]
    [Export(PropertyHint.File, "*.tscn")]
    private string _titleScene = null!;

    public override void _Ready()
    {
        HideHud();

        var settingsButton = GetNode<BaseButton>("%SettingsButton");
        var d1 = settingsButton.PressedAsObservable().Subscribe(_ => { SettingsHud.ShowHud(); });

        var resumeButton = GetNode<BaseButton>("%ResumeButton");
        var d2 = resumeButton.PressedAsObservable().Subscribe(_ => { HideHud(); });

        var titleButton = GetNode<BaseButton>("%TitleButton");
        var d3 = titleButton.PressedAsObservable().Subscribe(_ =>
        {
            HideHud();
            SceneManager.GoToScene(_titleScene);
        });

        var exitButton = GetNode<BaseButton>("%ExitButton");
        var d4 = exitButton.PressedAsObservable().Subscribe(_ =>
        {
            // Exit application
            GetTree().Quit();
        });

        Disposable.Combine(d1, d2, d3, d4).AddTo(this);
    }

    public override void _Input(InputEvent inputEvent)
    {
        // 設定の Tab が開いている場合は何もしない
        if (SettingsHud.IsVisible)
        {
            return;
        }

        if (inputEvent.IsActionPressed("open_pose"))
        {
            // 現在 タイトルシーンにいる場合は表示しない
            var root = GetTree().Root;
            var title = root.GetNodeOrNull("Title");
            if (title is not null)
            {
                return;
            }

            if (Visible)
            {
                HideHud();
            }
            else
            {
                ShowHud();
            }
        }
    }

    private void HideHud()
    {
        Hide();
        GetTree().Paused = false;
    }

    private void ShowHud()
    {
        // Pause Game
        GetTree().Paused = true;
        Show();
    }
}