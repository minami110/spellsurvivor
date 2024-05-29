using Godot;
using R3;

namespace fms;

public sealed partial class ShopHUD : CanvasLayer
{
    [ExportGroup("Internal Reference")]
    [Export]
    private ShopManager _shopManager = null!;

    private readonly Subject<Unit> _closed = new();
    public Observable<Unit> Closed => _closed;

    public override void _Ready()
    {
        _closed.AddTo(this);
    }

    public void InitialRerollItems()
    {
        _shopManager.Reroll();
    }

    public void OnCloseButtonPressed()
    {
        _closed.OnNext(Unit.Default);
    }
}