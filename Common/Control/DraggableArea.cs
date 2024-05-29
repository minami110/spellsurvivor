using fms;
using Godot;
using R3;

public partial class DraggableArea : Control
{
    private Vector2 _dragOffset;
    private bool _isDragging;
    private bool _mouseEntered;

    public override void _Ready()
    {
        this.MouseEnteredAsObservable().Subscribe(_ => _mouseEntered = true);
        this.MouseExitedAsObservable().Subscribe(_ => _mouseEntered = false);
    }

    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() + _dragOffset;
        }
    }

    public override void _GuiInput(InputEvent ev)
    {
        if (!_mouseEntered)
        {
            return;
        }

        if (ev is InputEventMouseButton mb)
        {
            _dragOffset = GlobalPosition - GetGlobalMousePosition();
            _isDragging = mb.IsPressed();
        }
    }
}