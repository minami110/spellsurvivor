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
    }

    // アニメーションの最初のタメ が終わったタイミングで呼ばれるコールバック
    private void A()
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");

        // この時点で狙う方向が確定する
        // 敵を殺したときに変な回転をしないように physics_process を止める 
        aim.SetPhysicsProcess(false);

        // 現在狙っている敵を取得
        var enemy = aim.NearestEnemy;
        if (enemy is null)
        {
            // ToDo: 狙っている間に敵が殺されたパターン
            return;
        }

        // アニメーションに合うようにエリア攻撃の弾を生成する
        var prj = _projectile.Instantiate<BaseProjectile>();

        // 敵の方向を向くような rotation を計算する
        var dir = enemy.GlobalPosition - GlobalPosition;
        var angle = dir.Angle();

        // 自分の位置から angle 方向に 90 伸ばした位置を計算する
        // Note: プレイ間確かめながらスポーン位置のピクセル数は調整する
        var pos = GlobalPosition + dir.Normalized() * 90;

        AddProjectile(prj, pos, angle);
    }

    // アニメーションの終了時に呼ばれるコールバック
    private void B()
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        aim.SetPhysicsProcess(true);
    }
}