using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// ポイズンミストを生成する
/// </summary>
public partial class PoisonMist : WeaponBase
{
    /// <summary>
    /// 毒沼の生存フレーム
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _aliveFrame = 300u;

    /// <summary>
    /// 毒沼の攻撃判定の間隔
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _attackSpan = 15u;

    /// <summary>
    /// 毒沼の大きさ
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:px")]
    private uint _size = 100u;

    private protected override void OnCoolDownComplete(uint level)
    {
        var prj = new CircleAreaProjectile();
        prj.Damage = BaseDamage;
        prj.Knockback = Knockback;
        prj.LifeFrame = _aliveFrame;
        prj.DamageEveryXFrames = _attackSpan;

        // レベルに応じて毒沼のサイズを変更する
        prj.Radius = level switch
        {
            <= 1 => _size,
            2 => _size + 20,
            >= 3 => _size + 50
        };
        prj.Offset = Vector2.Zero;

        AddProjectile(prj, GlobalPosition);
    }
}