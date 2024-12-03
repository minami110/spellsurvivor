using Godot;

namespace fms.HUD;

public partial class WeaponStatInfo : PanelContainer
{
    public WeaponStatusType StatType
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    } = 0u;

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (StatType == 0u)
        {
            Hide();
            return;
        }

        var name = GetNode<Label>("%Name");
        name.Text = $"STAT_{StatType.ToString().ToSnakeCase().ToUpper()}";

        var desc = GetNode<Label>("%Description");
        desc.Text = $"STAT_{StatType.ToString().ToSnakeCase().ToUpper()}_DESC";

        ResetSize();
        Show();
    }
}