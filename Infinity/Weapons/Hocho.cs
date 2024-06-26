﻿using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class Hocho : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override async void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.NearestEnemy;

        // ToDo: BaseCoolDown = 30f 決め打ちのアニメ を再生している
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(A));
        t.TweenProperty(sprite, "position", new Vector2(57, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.08d);
        t.TweenCallback(Callable.From(B));

        // ToDo: アニメ の最初の タメ を待って 弾を打っている
        await this.WaitForSecondsAsync(0.2d);

        if (IsInstanceValid(enemy))
        {
            var prj = _projectile.Instantiate<BaseProjectile>();
            AddProjectile(prj, GlobalPosition, enemy!.GlobalPosition - GlobalPosition);
        }
    }

    private void A()
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        aim.SetPhysicsProcess(false);
    }

    private void B()
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        aim.SetPhysicsProcess(true);
    }
}