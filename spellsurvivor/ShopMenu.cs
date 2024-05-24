using Godot;

namespace fms;

public partial class ShopMenu : CanvasLayer
{
    [Signal]
    public delegate void ClosedEventHandler();

    /// <summary>
    ///     Open ShopMenu
    /// </summary>
    public void Open()
    {
        Show();
    }

    /// <summary>
    ///     Close ShopMenu
    /// </summary>
    public void Close()
    {
        Hide();
        EmitSignal(SignalName.Closed);
    }
}