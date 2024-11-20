using Godot;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class HookedKnife : Hocho
{
    // 攻撃前のナイフを構えるアニメーションの距離
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _preAttackDistance = 10;

    // 攻撃前のナイフを構えるアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _preAttackDuration = 4;

    // 突き刺しアニメーションの距離
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushDuration = 10;

    private protected override async void OnCoolDownCompleted(uint level)
    {
        if (!AimToNearEnemy.IsAiming)
        {
            return;
        }

        // 包丁のひきはじめ, 通常時よりも感度を下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();

        // A. 攻撃前のナイフを構えるアニメーション
        var dulA = _preAttackDuration / 60d;
        var distA = _preAttackDistance * -1f;
        t.TweenProperty(sprite, "position", new Vector2(distA, 0), dulA)
            .SetEase(Tween.EaseType.Out);

        // B. 突き刺しアニメーション
        var dulB = _pushDuration / 60d;
        var distB = _pushDistance;
        t.TweenCallback(Callable.From(() =>
        {
            // 突き刺し時は回転しないようにする
            AimToNearEnemy.RotateSensitivity = 0.001f;
            // ダメージを有効化
            StaticDamage.Disabled = false;
        }));
        t.TweenProperty(sprite, "position", new Vector2(distB, 0), dulB)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        // C. 手元に戻す
        t.TweenCallback(Callable.From(() =>
        {
            // ダメージを無効化
            StaticDamage.Disabled = true;
        }));
        // 手元に戻すアニメーション
        t.TweenProperty(sprite, "position", new Vector2(3, 0), 0.08d);
        // すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // 再生終了したら AimToNearEnemy を再開する
        await ToSignal(t, Tween.SignalName.Finished);
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
        RestartCoolDown();
    }
}