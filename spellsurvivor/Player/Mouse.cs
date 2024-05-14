using Godot;
using System;
using Godot.Collections;

namespace spellsurvivor;

public partial class Mouse : Node2D
{
    public override void _Ready()
    {
        // Hide Mouse Cursor
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    public override void _Input(InputEvent inputEvent)
    {
        // Mouse in viewport coordinates.
        if (inputEvent is not InputEventMouseMotion eventMouseMotion)
        {
            return;
        }

        // Update MousePosition
        var viewportCenter = GetViewportRect().Size / 2f;
        Position = eventMouseMotion.Position - viewportCenter;
    }
}