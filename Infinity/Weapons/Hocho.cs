using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class Hocho : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.NearestEnemy;

        var prj = _projectile.Instantiate<BaseProjectile>();
        {
            prj.GlobalPosition = GlobalPosition;
            prj.Direction = enemy!.GlobalPosition - GlobalPosition;
        }

        FrameTimer.AddChild(prj);

        // ToDo:
        // BaseCoolDown = 30f 決め打ちのアニメ

        // Sprite 
        var sprite = GetNode<Node2D>("%Sprite");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(12, 0), 0.05d);
        t.TweenProperty(sprite, "position", new Vector2(70, 0), 0.3d)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "position", new Vector2(24, 0), 0.1d);
    }
}