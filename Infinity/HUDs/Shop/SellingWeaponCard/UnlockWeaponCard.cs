using Godot;

namespace fms.HUD;

[Tool]
public partial class UnlockWeaponCard : FmsButton
{
    [Export]
    private uint Price
    {
        get;
        set
        {
            field = value;
            UpdatePriceUi();
        }
    } = 5u;

    public override void _Ready()
    {
        UpdatePriceUi();
    }

    private void UpdatePriceUi()
    {
        if (IsNodeReady())
        {
            GetNode<Label>("%Price").Text = $"{Price}";
        }
    }
}