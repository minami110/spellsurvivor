using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// Incandescent の効果により生成される周囲の敵に定期的にダメージを与える武器
/// </summary>
public partial class Heat : WeaponBase
{
    /// <summary>
    /// 生成するダメージの半径
    /// </summary>
    [Export(PropertyHint.Range, "0,9999")]
    public uint Radius
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                var sprite = GetNode<Sprite2D>("Sprite");
                const uint _IMAGE_SIZE = 128; // ToDo: 画像サイズを取得する
                var scale = Radius * 2f / _IMAGE_SIZE;
                sprite.Scale = new Vector2(scale, scale);
            }
        }
    } = 100u;

    public override void _Ready()
    {
        var sprite = GetNode<Sprite2D>("Sprite");
        const int _IMAGE_SIZE = 128; // ToDo: 画像サイズを取得する
        var scale = Radius * 2f / _IMAGE_SIZE;
        sprite.Scale = new Vector2(scale, scale);
    }

    private protected override void OnCoolDownComplete(uint level)
    {
        // 円形の攻撃を行う
        var prj = new CircleAreaProjectile();
        {
            prj.Damage = Damage;
            prj.Knockback = Knockback;
            prj.LifeFrame = 5u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えて消滅する
            prj.Radius = Radius;
            prj.Offset = Vector2.Zero;
        }

        AddProjectile(prj, GlobalPosition);
    }
}