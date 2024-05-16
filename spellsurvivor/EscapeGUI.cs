using Godot;
using System;
using System.Diagnostics;

public partial class EscapeGUI : Node2D
{
    private bool _isShowing = false;

    public void OnExitPressed()
    {
        // Exit application
        GetTree().Quit();
    }

    public void OnResumePressed()
    {
            GD.Print("Resume Game");
            
        if (_isShowing)
        {
            _isShowing = false;
            GetTree().Paused = false;
            Hide();
        }
    }

    public override void _Ready()
    {
        _isShowing = false;
        Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("open_pose"))
        {
            if (_isShowing)
            {
                _isShowing = false;
                GetTree().Paused = false;
                Hide();
            }
            else
            {
                _isShowing = true;
                // Pause Game
                GetTree().Paused = true;
                Show();
            }
        }
    }
}