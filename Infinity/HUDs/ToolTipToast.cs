using System.Runtime.CompilerServices;
using Godot;

namespace fms;

public partial class ToolTipToast : PanelContainer
{
    [Export]
    private Label _label = null!;

    [Export]
    private Vector2 _mouseOffset = new(10, 10);

    private static ToolTipToast? _singleton;

    public static string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _singleton is null ? string.Empty : _singleton._label.Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (_singleton is null)
            {
                return;
            }

            _singleton._label.Text = value;
        }
    }

    public override void _EnterTree()
    {
        _singleton = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        // マウスカーソルの位置を追従する
        if (@event is InputEventMouseMotion mouseMotion)
        {
            Position = mouseMotion.Position + _mouseOffset;
        }
    }

    public override void _ExitTree()
    {
        _singleton = null;
    }

    public new static void Hide()
    {
        if (_singleton is null)
        {
            return;
        }

        ((CanvasItem)_singleton).Hide();
    }

    public new static void Show()
    {
        if (_singleton is null)
        {
            return;
        }

        // Update container Size


        _singleton.ResetSize();
        ((CanvasItem)_singleton).Show();
    }
}