using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
///     ポイズンミストを生成する
/// </summary>
public partial class PoisonMistGen : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        switch (level)
        {
            case 2:
                prj.AddChild(new SizeMod { Add = 100 }); // 100 => 200
                break;
            case 3:
                prj.AddChild(new SizeMod { Add = 200 }); // 100 => 300
                break;
        }

        AddProjectile(prj, GlobalPosition);
    }
}