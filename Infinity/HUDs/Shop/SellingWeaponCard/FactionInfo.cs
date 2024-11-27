using Godot;

namespace fms.HUD;

[Tool]
public partial class FactionInfo : HBoxContainer
{
    [Export]
    public string Faction
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                UpdateUI();
            }
        }
    } = string.Empty;

    public override void _Ready()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Faction != string.Empty)
        {
            Show();
            GetNode<Label>("%Faction").Text = $"FACTION_{Faction.ToUpper()}";
        }
        else
        {
            Hide();
        }
    }
}