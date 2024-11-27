using Godot;

namespace fms.HUD;

[Tool]
public partial class LockButton : Button
{
    [Export]
    private bool Locked
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            if (IsNodeReady())
            {
                UpdateUi();
            }
        }
    } = false;

    public override void _Ready()
    {
        UpdateUi();
    }

    private void UpdateUi()
    {
        if (Locked)
        {
            GetNode<TextureRect>("UnlockedSprite").Hide();
            GetNode<TextureRect>("LockedSprite").Show();
        }
        else
        {
            GetNode<TextureRect>("UnlockedSprite").Show();
            GetNode<TextureRect>("LockedSprite").Hide();
        }
    }
}