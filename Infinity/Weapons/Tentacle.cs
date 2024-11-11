using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// 触手を生成する武器
/// </summary>
public partial class Tentacle : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();

        // プレイヤーの位置にスポーンさせる
        AddProjectile(prj, GlobalPosition);
    }
}