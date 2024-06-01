using Godot;
using R3;

namespace fms;

public sealed partial class ShopHUD : CanvasLayer
{
    [ExportGroup("Internal Reference")]
    [Export]
    private ShopManager _shopManager = null!;

    public override void _Ready()
    {
        var ws = Main.WaveState;
        var d5 = ws.Phase.Subscribe(p =>
        {
            if (p == WavePhase.SHOP)
            {
                Show();
                _shopManager.Reroll();
            }
            else
            {
                Hide();
            }
        });
    }

    public void OnCloseButtonPressed()
    {
        Main.WaveState.SendSignal(WaveState.Signal.PLAYER_ACCEPTED_SHOP);
    }
}