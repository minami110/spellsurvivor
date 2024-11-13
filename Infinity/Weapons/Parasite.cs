using Godot;

namespace fms.Weapon;

public partial class Parasite : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    /// <summary>
    /// Parasite の攻撃間隔
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _attackSpan = 15u;

    /// <summary>
    /// パラサイトの移動速度
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:px/s")]
    private float _speed = 120f;

    /// <summary>
    /// 移動速度の個体差の ± の幅
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _randomSpeed = 20f;

    [Export]
    private uint _searchRadius = 150u;

    [Export]
    private uint _damageRadius = 20u;

    /// <summary>
    /// ウェーブ開始時に呼ばれるコールバック
    /// </summary>
    private protected override void OnStartAttack()
    {
        // レベルに応じて数が増える
        for (var i = 0; i < Level; i++)
        {
            var prj = _projectile.Instantiate<ParasiteAreaDamage>();
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.DamageEveryXFrames = _attackSpan;

            var speed = _speed + (float)GD.Randfn(0f, _randomSpeed);
            speed = Mathf.Max(speed, 0f);
            prj.FollowSpeed = _speed + (float)GD.Randfn(0f, speed); // 個体差をもたせると狙う敵がバラける
            prj.SearchRadius = _searchRadius;
            prj.DamageRadius = _damageRadius;

            AddProjectile(prj, GlobalPosition);
        }
    }
}