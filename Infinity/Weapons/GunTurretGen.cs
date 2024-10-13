using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
///     ガンタレットを生成する
/// </summary>
public partial class GunTurretGen : WeaponBase
{
    [Export]
    private PackedScene _turret = null!;
    
    private uint _baseCoolDownFrame = 10u;

    // Projectileじゃないけど、WeaponBaseの機構にタダ乗りするためにSpawnProjectileメソッドを利用
    private protected override void SpawnProjectile(uint level)
    {
        // GunTurretを生成する。
        // 今、味方のNPCを呼び出す共通機構がないためとりあえず個別実装で対応
        var turret = _turret.Instantiate<GunTurret>();
        //turret.AddChild();
    }
}
