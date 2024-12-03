using Godot;

namespace fms.HUD;

[Tool]
public partial class WeaponDescriptionToast : Control
{
    [Export]
    public string Header
    {
        get;
        set
        {
            field = value;
            UpdateHeaderUi();
        }
    } = string.Empty;

    [Export]
    public string Description
    {
        get;
        set
        {
            field = value;
            UpdateDescriptionUi();
        }
    } = string.Empty;

    public override void _Ready()
    {
        UpdateHeaderUi();
    }

    private void UpdateDescriptionUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        GetNode<RichTextLabel>("%Description").Text = Description;
    }

    private void UpdateHeaderUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        GetNode<Label>("%Header").Text = Header;
    }
}