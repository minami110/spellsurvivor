using Godot;
using R3;

namespace fms;

public readonly struct EnemyDamageStatic
{
    public required Vector2 Position { get; init; }
    public required bool IsDead { get; init; }
}

/// <summary>
///     統計を収集するためのクラス
///     ダメージ表示も担当している
/// </summary>
public partial class StaticsManager : CanvasLayer
{
    [Export]
    private PackedScene _damageNumberScene = null!;

    [Export]
    private Node? _customDamageNumberContainer;

    public enum DamageTakeOwner
    {
        Player,
        Enemy
    }

    private static StaticsManager? _instance;

    private readonly Subject<EnemyDamageStatic> _enemyDamageOccurred = new();

    /// <summary>
    /// </summary>
    public static Observable<EnemyDamageStatic> EnemyDamageOccurred =>
        _instance?._enemyDamageOccurred ?? Observable.Empty<EnemyDamageStatic>();

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            _instance = this;
        }
        else if (what == NotificationExitTree)
        {
            _instance = null;
            _enemyDamageOccurred.Dispose();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="ownerType"></param>
    /// <param name="amount"></param>
    /// <param name="globalPosition"></param>
    public static void CommitDamage(DamageTakeOwner ownerType, float amount, in Vector2 globalPosition,
        bool isDead = false)
    {
        if (_instance is null)
        {
            return;
        }

        if (_instance.IsQueuedForDeletion() || !IsInstanceValid(_instance))
        {
            return;
        }

        _instance.SendDamageInternal(ownerType, amount, globalPosition, isDead);
    }

    private void SendDamageInternal(DamageTakeOwner ownerType, float damage, in Vector2 position, bool isDead)
    {
        // Config で表示設定がされていない場合は表示しない
        if (!GameConfig.Singleton.ShowDamageNumbers.Value)
        {
            return;
        }

        var damageNumberHud = _damageNumberScene.Instantiate<DamageNumberHud>();
        damageNumberHud.Hide();
        damageNumberHud.Damage = damage;
        // Player
        if (ownerType == DamageTakeOwner.Player)
        {
            damageNumberHud.PhysicalDamageColor = new Color(1f, 0.5f, 0.5f);
            damageNumberHud.HealColor = new Color(0.5f, 1.0f, 0.5f);
        }
        // Enemy
        else
        {
            _enemyDamageOccurred.OnNext(new EnemyDamageStatic { Position = position, IsDead = isDead });
            damageNumberHud.PhysicalDamageColor = new Color(1f, 1f, 1f);
        }

        if (_customDamageNumberContainer is not null)
        {
            _customDamageNumberContainer.AddChild(damageNumberHud);
        }
        else
        {
            AddChild(damageNumberHud);
        }

        damageNumberHud.GlobalPosition = position;
        damageNumberHud.Show();
    }
}