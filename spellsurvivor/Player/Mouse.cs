using Godot;

namespace fms;

public partial class Mouse : Node2D
{
    public override void _Process(double _)
    {
        GlobalPosition = GetGlobalMousePosition();
    }
}