using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// 振りかぶりタイプの片刃切りつけアニメーションを持つ近接武器
/// </summary>
public partial class Katana : WeaponBase
{
    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 100f;

    /// <summary>
    /// 敵を狙う速度の感度 (0 ~ 1), 1 で最速, 0 で全然狙えない
    /// </summary>
    [Export(PropertyHint.Range, "0.01,1.0,0.01")]
    private float _rotateSensitivity = 0.3f;

    private AimToNearEnemy AimToNearEnemy => GetNode<AimToNearEnemy>("AimToNearEnemy");

    private StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override void _Ready()
    {
        AimToNearEnemy.SearchRadius = _maxRange;
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        if (AimToNearEnemy.IsAiming)
        {
            PlayAttackAnimation();
        }
        else
        {
            AimToNearEnemy.EnteredAnyEnemy
                .Take(1)
                .Subscribe(this, (_, state) => { state.PlayAttackAnimation(); })
                .AddTo(this);
        }
    }

    private protected override void OnStartAttack(uint level)
    {
        StaticDamage.Disabled = true;
    }

    private void PlayAttackAnimation()
    {
        // 包丁のひきはじめ, 通常時よりも感度を下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
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
            StaticDamage.Damage = State.Damage.CurrentValue;
            StaticDamage.Knockback = State.Knockback.CurrentValue;
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
            .Subscribe(this, (_, state) =>
            {
                state.AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
                state.RestartCoolDown();
            })
            .AddTo(this);
    }
}