﻿using System.Threading.Tasks;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class AorticKnife : WeaponBase
{
    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 80f;

    /// <summary>
    /// 敵を狙う速度の感度 (0 ~ 1), 1 で最速, 0 で全然狙えない
    /// </summary>
    [Export(PropertyHint.Range, "0.01,1.0,0.01")]
    private protected float _rotateSensitivity = 0.3f;

    /// <summary>
    /// 突き刺し回数
    /// </summary>
    [Export(PropertyHint.Range, "1,10")]
    private int PushCount
    {
        get;
        set => field = Mathf.Clamp(value, 1, 10);
    } = 3;

    // 突き刺しアニメーションの距離
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushDuration = 10;

    // 攻撃前のナイフを構えるアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _preAttackDuration = 10;

    private AimToNearEnemy AimToNearEnemy => GetNode<AimToNearEnemy>("AimToNearEnemy");
    private StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override void _Ready()
    {
        AimToNearEnemy.SearchRadius = _maxRange;
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
    }

    private protected override async ValueTask OnCoolDownCompletedAsync(uint level)
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
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(() =>
        {
            // 突き刺し時は回転しないようにする
            AimToNearEnemy.RotateSensitivity = 0.001f;
        }));

        // 突き刺しアニメーション
        for (var i = 0; i < PushCount; i++)
        {
            Push(t, sprite);
        }

        // すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // 再生終了したら回転を再開する
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state.AimToNearEnemy.RotateSensitivity = _rotateSensitivity; })
            .AddTo(this);

        await ToSignal(t, Tween.SignalName.Finished);
    }

    private protected override void OnStartAttack()
    {
        StaticDamage.Disabled = true;
        StaticDamage.Damage = BaseDamage;
        StaticDamage.Knockback = Knockback;
    }

    private void Push(Tween tween, Node2D sprite)
    {
        // 突き刺しアニメーション
        var distance = (float)_pushDistance;
        var totalDulation = _pushDuration / 60f;
        var backDulation = 0.08f;
        var pushDulation = Mathf.Max(totalDulation - 0.08f, 0.01f);

        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = false; })); // ダメージを有効化
        tween.TweenProperty(sprite, "position", new Vector2(distance, 0), pushDulation)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        // 手元に戻すアニメーション
        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = true; })); // ダメージを無効化
        tween.TweenProperty(sprite, "position", new Vector2(3, 0), backDulation);
    }
}