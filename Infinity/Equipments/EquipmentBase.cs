using Godot;
using R3;

namespace fms;

public partial class EquipmentBase : Node2D
{
    [Export]
    private protected Timer Timer = null!;

    private readonly ReactiveProperty<float> _remainingCoolDownTime = new(0f);

    public ShopItemSettings ItemSettings { get; set; } = null!;

    public ReadOnlyReactiveProperty<float> RemainingCoolDownTime => _remainingCoolDownTime;

    public override void _Ready()
    {
        _remainingCoolDownTime.AddTo(this);
    }

    public override void _Process(double delta)
    {
        // TODO: ポーリングしてるのでなんとかする
        _remainingCoolDownTime.Value = (float)Timer.TimeLeft;
    }
}