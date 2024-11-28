using Godot;

namespace fms.HUD;

public partial class FmsButton : Button
{
    [Export]
    public Color EnabledColor { get; set; } = Colors.White;

    [Export]
    public Color DisabledColor { get; set; } = new(0.5f, 0.5f, 0.5f);

    public new bool Disabled
    {
        get => base.Disabled;
        set
        {
            base.Disabled = value;
            Modulate = value ? DisabledColor : EnabledColor;
        }
    }
}