using Godot;
using R3;

namespace fms.HUD;

public partial class ShopHudController : Node
{
    [Export]
    private Control _container = null!;

    [Export]
    private Button _quitShopButton = null!;

    public override void _Ready()
    {
        // ボタンのバインドを更新
        var d02 = _quitShopButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedShop);
        });

        // WaveState
        var d40 = Main.WaveState.Phase.Subscribe(this, (x, state) =>
        {
            if (x == WavePhase.Shop)
            {
                state._container.Show();
            }
            else
            {
                state._container.Hide();
            }
        });

        Disposable.Combine(d02, d40).AddTo(this);
    }
}