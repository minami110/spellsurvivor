using Godot;

namespace fms.HUD;

/// <summary>
/// 小要素すべてを暗くすることができるボタン
/// Note: 公式が Disabled の override も Signal も用意していないのでこのようなハメになっている
/// </summary>
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