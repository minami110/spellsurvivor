using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class SickleSword : Hocho
{
    private protected override async void OnCoolDownCompleted(uint level)
    {
        if (!AimToNearEnemy.IsAiming)
        {
            return;
        }

        // 包丁のひきはじめ, 通常時よりも感度を下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
        // ToDo: 固定長のアニメーションなので, BaseCoolDown のほうが早くなるとおかしくなる 
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();
        t.SetParallel();

        // 構え
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.1d)
            .SetEase(Tween.EaseType.Out);

        // なぎ開始
        t.Chain();
        t.TweenCallback(Callable.From(() =>
        {
            // 突き刺し時は回転しないようにする
            AimToNearEnemy.RotateSensitivity = 0.01f;
            // ダメージを有効化
            StaticDamage.Disabled = false;
        }));
        t.TweenProperty(sprite, "position", new Vector2(5, -30), 0.05d)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "rotation", Mathf.DegToRad(-120), 0.05d)
            .SetEase(Tween.EaseType.Out);

        t.Chain();
        t.TweenProperty(sprite, "position", new Vector2(65, 50), 0.1d)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "rotation", Mathf.DegToRad(50), 0.1d)
            .SetEase(Tween.EaseType.Out);

        // なぎ終了
        t.Chain();
        t.TweenCallback(Callable.From(() =>
        {
            // ダメージを無効化
            StaticDamage.Disabled = true;
        }));
        // 手元に戻すアニメーション
        t.TweenProperty(sprite, "position", new Vector2(3, 0), 0.08d);
        t.TweenProperty(sprite, "rotation", Mathf.DegToRad(-4), 0.08d);

        t.Chain();
        // すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);
        t.TweenProperty(sprite, "rotation", 0f, 0.2d);

        // 再生終了したら AimToNearEnemy を再開する
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state.AimToNearEnemy.RotateSensitivity = _rotateSensitivity; })
            .AddTo(this);

        await ToSignal(t, Tween.SignalName.Finished);
        RestartCoolDown();
    }
}