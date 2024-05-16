using Godot;

public partial class DebugGUI : Node2D
{
    public override void _Process(double delta)
    {
        // GetFPS
        var fps = Engine.GetFramesPerSecond();
        GetNode<Label>("FPS").Text = $"FPS: {fps}";
    }
}