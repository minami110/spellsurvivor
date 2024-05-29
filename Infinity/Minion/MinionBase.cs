using Godot;
using R3;

namespace fms;

/// <summary>
///     Minion のベースクラス
/// </summary>
public partial class MinionBase : Node2D
{
    [Export]
    private protected Timer Timer = null!;

    private readonly ReactiveProperty<float> _remainingCoolDownTime = new(0f);

    /// <summary>
    ///     Gets the level of this minion.
    /// </summary>
    public int Level { get; private set; }

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