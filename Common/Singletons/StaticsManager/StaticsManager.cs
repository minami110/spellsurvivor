using Godot;
using R3;

namespace fms;

public readonly struct DamageReport
{
    // ダメージを発生させた Entity
    public required IEntity? Instigator { get; init; }

    // ダメージを受けた Entity
    public required IEntity Victim { get; init; }

    // 発生したダメージ量
    public required float Amount { get; init; }

    // ダメージが発生した Global Position
    public required Vector2 Position { get; init; }

    // ダメージを発生させた Node
    public required Node Causer { get; init; }

    // Player/TurretWeapon/Turret など Causer の 所属が確認できるパス
    public required string CauserPath { get; init; }

    /// <summary>
    /// 死亡要因になった Damage の場合 true
    /// </summary>
    public required bool IsVictimDead { get; init; }
}

/// <summary>
/// 統計を収集するためのクラス
/// ダメージ表示も担当している
/// </summary>
public partial class StaticsManager : CanvasLayer
{
    [Export]
    private PackedScene _damageNumberScene = null!;

    [Export]
    private PackedScene _dodgePopup = null!;

    [Export]
    private Node? _customDamageNumberContainer;

    public enum DamageTakeOwner
    {
        Player,
        Enemy
    }

    private static StaticsManager? _instance;
    private readonly Subject<DamageReport> _reportedDamage = new();


    /// <summary>
    /// 敵がダメージを受けた際に発生するイベント
    /// </summary>
    public static Observable<DamageReport> ReportedDamage =>
        _instance?._reportedDamage ?? Observable.Empty<DamageReport>();

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            _instance = this;
        }
        else if (what == NotificationExitTree)
        {
            _instance = null;
            _reportedDamage.Dispose();
        }
    }

    /// <summary>
    /// </summary>
    public static void ReportDamage(in DamageReport report)
    {
        if (_instance is null || !IsInstanceValid(_instance) || _instance.IsQueuedForDeletion())
        {
            return;
        }

        _instance._reportedDamage.OnNext(report);

        var victim = report.Victim;

        // プレイヤーがダメージを受けた場合 
        if (victim is EntityPlayer)
        {
            _instance.PopUpDamageHud(DamageTakeOwner.Player, report.Amount, report.Position, report.IsVictimDead);
        }
        // 敵がダメージを受けた場合
        else if (victim is EntityEnemy)
        {
            _instance.PopUpDamageHud(DamageTakeOwner.Enemy, report.Amount, report.Position, report.IsVictimDead);
        }
    }

    public static void SuccessDodge(in Vector2 position)
    {
        _instance?.PopUpDodgeHud(in position);
    }

    private void PopUpDamageHud(DamageTakeOwner ownerType, float damage, in Vector2 position, bool isDead)
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

    private void PopUpDodgeHud(in Vector2 position)
    {
        var popup = _dodgePopup.Instantiate<DodgeHud>();
        popup.Hide();

        if (_customDamageNumberContainer is not null)
        {
            _customDamageNumberContainer.AddChild(popup);
        }
        else
        {
            AddChild(popup);
        }

        popup.GlobalPosition = position;
        popup.Show();
    }
}