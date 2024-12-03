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

    [Export]
    protected string FocusKey { get; private set; } = string.Empty;

    public new bool Disabled
    {
        get => base.Disabled;
        set
        {
            base.Disabled = value;
            Modulate = value ? DisabledColor : EnabledColor;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationFocusEnter)
        {
            ToastManager.Instance.CommitFocusEntered(FocusKey);
        }
    }
}