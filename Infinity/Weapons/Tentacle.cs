using Godot;

namespace fms.Weapon;

/// <summary>
/// 触手を生成する武器
/// </summary>
public partial class Tentacle : WeaponBase
{
    /// <summary>
    /// 触手本体の PackedScene
    /// </summary>
    [ExportGroup("Body Settings")]
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _tentackeAliveFrame = 350u;

    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _tentackeAttackSpan = 200u;

    /// <summary>
    /// 触手が敵を検知する範囲 (px)
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:px")]
    private uint _enemyDetectionRadius = 85u;

    /// <summary>
    /// 生成するダメージエリアのサイズ
    /// </summary>
    [ExportGroup("Bullet Settings")]
    [Export]
    private Vector2 _damageSize = new(180, 70);

    private protected override void OnCoolDownCompleted(uint level)
    {
        var prj = _projectile.Instantiate<TentacleBody>();
        prj.Damage = State.Damage.CurrentValue;
        prj.Knockback = State.Knockback.CurrentValue;
        prj.LifeFrame = _tentackeAliveFrame; // 触手の生存フレーム
        prj.DamageEveryXFrames = _tentackeAttackSpan; // 触手が生成されてからダメージを与えるまでのフレーム (繰り返す)
        prj.DetectionRadius = _enemyDetectionRadius;
        prj.DamageSize = _damageSize;

        // プレイヤーの位置にスポーンさせる
        AddProjectile(prj, GlobalPosition);
        RestartCoolDown();
    }
}