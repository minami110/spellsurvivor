using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// ダガータイプの突き刺しアニメーションを持つ近接武器
/// </summary>
public partial class Hocho : WeaponBase
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

    /// <summary>
    /// 突き刺し回数
    /// </summary>
    [Export(PropertyHint.Range, "1,10")]
    private int PushCount
    {
        get;
        set => field = Mathf.Clamp(value, 1, 10);
    } = 1;

    // 攻撃前の構えるアニメーションの距離
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _preAttackDistance = 10;

    // 攻撃前の構えるアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _preAttackDuration = 4;

    // 突き刺しアニメーションの距離
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushDuration = 10;

    private AimToNearEnemy AimToNearEnemy => GetNode<AimToNearEnemy>("AimToNearEnemy");

    private protected StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

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

    private protected override void OnStartAttack()
    {
        StaticDamage.Disabled = true;
        StaticDamage.Damage = Damage;
        StaticDamage.Knockback = Knockback;
    }

    private void PlayAttackAnimation()
    {
        // A. 包丁のひきはじめ, 通常時よりも感度を下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();

        // A. 攻撃前のナイフを構えるアニメーション
        {
            var dist = _preAttackDistance * -1f;
            var dul = _preAttackDuration / 60d;
            t.TweenProperty(sprite, "position", new Vector2(dist, 0), dul)
                .SetEase(Tween.EaseType.InOut);
        }

        // B. ナイフを前に突き出すアニメーション, この段階でエイム感度をほぼ 0 にする
        {
            t.TweenCallback(Callable.From(() => { AimToNearEnemy.RotateSensitivity = 0.001f; }));
            for (var i = 0; i < PushCount; i++)
            {
                RegisterPushAnimation(t, sprite);
            }
        }

        // C. すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // D. Tween の終了後の処理
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) =>
            {
                // 通常の感度に戻す
                state.AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
                state.RestartCoolDown();
            })
            .AddTo(this);
    }

    private void RegisterPushAnimation(Tween tween, Node2D sprite)
    {
        // 突き刺しアニメーション
        var dist = (float)_pushDistance;
        var totalDulation = _pushDuration / 60d;
        var backDulation = 0.08f;
        var pushDulation = Mathf.Max(totalDulation - 0.08f, 0.01f);

        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = false; })); // ダメージを有効化
        tween.TweenProperty(sprite, "position", new Vector2(dist, 0), pushDulation)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);

        // 手元に戻すアニメーション
        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = true; })); // ダメージを無効化
        tween.TweenProperty(sprite, "position", new Vector2(3, 0), backDulation);
    }
}