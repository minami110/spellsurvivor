using Godot;

namespace fms.Weapon;

/// <summary>
/// 触手を生成する武器
/// </summary>
public partial class Tentacle : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    /// <summary>
    /// 生成するダメージエリアのサイズ
    /// </summary>
    [Export]
    private Vector2 _damageSize = new(180, 70);

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<TentacleBody>();
        prj.Damage = BaseDamage;
        prj.Knockback = Knockback;
        prj.LifeFrame = 350u; // 触手の生存フレーム
        prj.DamageEveryXFrames = 200u; // 触手が生成されてからダメージを与えるまでのフレーム (繰り返す)
        prj.DamageSize = _damageSize;

        // プレイヤーの位置にスポーンさせる
        AddProjectile(prj, GlobalPosition);
    }
}