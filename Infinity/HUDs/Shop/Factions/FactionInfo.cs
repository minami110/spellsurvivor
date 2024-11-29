using fms.Faction;
using Godot;

public partial class FactionInfo : Control
{
    [Export]
    public FactionType Faction
    {
        get;
        set
        {
            field = value;
            OnChangedFaction();
        }
    }

    private void OnChangedFaction()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Faction == 0u)
        {
            Hide();
            return;
        }

        GetNode<TextureRect>("%Sprite").Texture = Faction.GetTextureResouce();
        GetNode<Label>("%Name").Text = $"FACTION_{Faction.ToString().ToUpper()}";

        // ToDo:

        Show();
    }
}