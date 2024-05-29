using Godot;
using R3;

namespace fms;

/// <summary>
///     Minion のベースクラス
/// </summary>
public partial class MinionBase : Node2D
{
    private readonly ReactiveProperty<float> _coolDownLeft = new(0f);
    private Timer _coolDownTimer = null!;

    /// <summary>
    ///     Gets the level of this minion.
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    ///     Gets the maximum level of this minion.
    /// </summary>
    public virtual int MaxLevel => 5;

    /// <summary>
    /// </summary>
    public ShopItemSettings ItemSettings { get; set; } = null!;

    /// <summary>
    ///     次の攻撃までの残り時間 (単位: 秒)
    /// </summary>
    public ReadOnlyReactiveProperty<float> CoolDownLeft => _coolDownLeft;

    public override void _Ready()
    {
        // Timer の初期化
        _coolDownTimer = new Timer();
        AddChild(_coolDownTimer);
        _coolDownTimer.WaitTime = ItemSettings.CoolDown;
        var d1 = Main.GameMode.WaveStarted.Subscribe(this, (_, s) => s._coolDownTimer.Start());
        var d2 = Main.GameMode.WaveEnded.Subscribe(this, (_, s) => s._coolDownTimer.Stop());
        var d3 = _coolDownTimer.TimeoutAsObservable().Subscribe(this, (_, s) => s.DoAttack());
        var d4 = _coolDownLeft;

        Disposable.Combine(d1, d2, d3, d4).AddTo(this);
    }

    public override void _Process(double delta)
    {
        // TODO: ポーリングしてるのでなんとかする
        _coolDownLeft.Value = (float)_coolDownTimer.TimeLeft;
    }

    private protected virtual void DoAttack()
    {
    }
}