using fms.Mob;
using Godot;

namespace fms.Weapon;

/// <summary>
/// ガンタレットを生成する
/// </summary>
public partial class GunTurret : WeaponBase
{
    /// <summary>
    /// タレット本体 の PackedScene
    /// </summary>
    [ExportGroup("Body Settings")]
    [Export]
    private PackedScene _body = null!;

    /// <summary>
    /// タレットの生存フレーム
    /// </summary>
    [Export]
    private uint _bodyAliveFrame = 600u;

    /// <summary>
    /// タレットの攻撃間隔
    /// </summary>
    [Export]
    private uint _bodyAttackSpan = 60u;

    /// <summary>
    /// タレット本体が敵を検知する範囲 (px)
    /// </summary>
    [Export]
    private uint _bodyEnemyDetectionRadius = 100u;

    [ExportGroup("Bullet Settings")]
    [Export]
    private PackedScene _bullet = null!;

    [Export]
    private uint _bulletAliveFrame = 15u;

    [Export]
    private uint _bulletSpeed = 800u;

    // Projectileじゃないけど、WeaponBaseの機構にタダ乗りするためにSpawnProjectileメソッドを利用
    private protected override void OnCoolDownCompleted(uint level)
    {
        // タレット本体を生成する
        var turret = _body.Instantiate<Turret>();
        turret.Lifetime = _bodyAliveFrame; // タレット本体の生存フレーム
        turret.AttackSpeed = _bodyAttackSpan; // タレット本体が攻撃を行う感覚
        turret.DetectionRadius = _bodyEnemyDetectionRadius;

        turret.BulletScene = _bullet;
        turret.Damage = State.Damage.CurrentValue;
        turret.Knockback = State.Knockback.CurrentValue;
        turret.BulletLifetime = _bulletAliveFrame;
        turret.BulletSpeed = _bulletSpeed;

        // ワールド位置にスポーンさせる
        // ToDO: あいまい
        var world = GetParent().GetParent();
        turret.GlobalPosition = GlobalPosition;
        world.AddChild(turret);
        RestartCoolDown();
    }
}