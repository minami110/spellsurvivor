using Godot;

namespace fms.HUD;

[Tool]
public partial class ShopActionButton : Button
{
    [Export]
    private string Title
    {
        get;
        set
        {
            field = value;
            UpdateTitleUi();
        }
    } = string.Empty;

    [Export]
    private uint Cost
    {
        get;
        set
        {
            field = value;
            UpdateCostUi();
        }
    }

    [Export]
    private Texture2D? Sprite
    {
        get;
        set
        {
            field = value;
            UpdateSpriteUi();
        }
    }

    public override void _Ready()
    {
        UpdateTitleUi();
        UpdateCostUi();
        UpdateSpriteUi();
    }

    private void UpdateCostUi()
    {
        if (IsNodeReady())
        {
            GetNode<Label>("%Cost").Text = $"{Cost}";
        }
    }

    private void UpdateSpriteUi()
    {
        if (IsNodeReady())
        {
            GetNode<TextureRect>("%Sprite").Texture = Sprite;
        }
    }

    private void UpdateTitleUi()
    {
        if (IsNodeReady())
        {
            GetNode<Label>("%Title").Text = Title;
        }
    }
}