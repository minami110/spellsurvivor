using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using R3;

namespace fms;

public readonly struct DamageReport
{
    public required float Amount { get; init; }
    public required Node Victim { get; init; }
    public required IEntity Instigator { get; init; }
    public required Node Causer { get; init; }
    public required string CauserType { get; init; }
    public required Vector2 Position { get; init; }
    public required bool IsDead { get; init; }
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

    private readonly Dictionary<string, List<DamageReport>> _damageInfoByCauser = new();

    private readonly Subject<DamageReport> _enemyDamageOccurred = new();

    /// <summary>
    /// Causer ごとのダメージ情報テーブル
    /// </summary>
    public static IReadOnlyDictionary<string, List<DamageReport>> DamageInfoByCauser
    {
        get
        {
            if (_instance is not null)
            {
                return _instance._damageInfoByCauser;
            }

            return ImmutableDictionary<string, List<DamageReport>>.Empty;
        }
    }

    /// <summary>
    /// 敵がダメージを受けた際に発生するイベント
    /// </summary>
    public static Observable<DamageReport> EnemyDamageOccurred =>
        _instance?._enemyDamageOccurred ?? Observable.Empty<DamageReport>();

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

    public static void ClearDamageInfoTable()
    {
        _instance?._damageInfoByCauser.Clear();
    }

    /// <summary>
    /// </summary>
    public static void CommitDamage(in DamageReport report)
    {
        if (_instance is null)
        {
            return;
        }

        if (_instance.IsQueuedForDeletion() || !IsInstanceValid(_instance))
        {
            return;
        }

        // ダメージを与えたもの (Causer) ごとにダメージ情報を保存する
        var causerType = report.CauserType;
        if (!_instance._damageInfoByCauser.TryGetValue(causerType, out var damageInfos))
        {
            damageInfos = new List<DamageReport>();
            _instance._damageInfoByCauser[causerType] = damageInfos;
        }

        damageInfos.Add(report);


        var victim = report.Victim;

        // プレイヤーがダメージを受けた場合 
        if (victim.IsInGroup(GroupNames.Player))
        {
            _instance.PopUpDamageHud(DamageTakeOwner.Player, report.Amount, report.Position, report.IsDead);
        }
        // 敵がダメージを受けた場合
        else if (victim.IsInGroup(Constant.GroupNameEnemy))
        {
            _instance.PopUpDamageHud(DamageTakeOwner.Enemy, report.Amount, report.Position, report.IsDead);
            _instance._enemyDamageOccurred.OnNext(report);
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