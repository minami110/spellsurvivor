using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fms.Effect;
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
    private float _rotateSensitivity = 0.3f;

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
    private uint _preAttackDuration = 4;

    private uint _lifestealAmount;
    private float _lifeStealRate;

    private AimToNearEnemy AimToNearEnemy => GetNode<AimToNearEnemy>("AimToNearEnemy");
    private StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override void _Ready()
    {
        AimToNearEnemy.SearchRadius = _maxRange;
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity;

        StaticDamage.Hit.Subscribe(payload =>
        {
            var hitEntity = (IEntity)(Node2D)payload["Entity"];

            // ToDo: ライフスティールの対象かどうか?
            // Player なら Enemy にあたったとき みたいな確認

            // ライフスティールの確率計算
            var chance = Math.Clamp(_lifeStealRate, 0f, 1f);
            if (GD.Randf() < chance)
            {
                // OwnedEntity (Player) を回復する
                var player = (IEntity)OwnedEntity;
                player.ApplayDamage(_lifestealAmount * -1f, player, this);
            }
        }).AddTo(this);
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        if (AimToNearEnemy.IsAiming)
        {
            _ = PlayAttackAnimationAsync();
        }
        else
        {
            AimToNearEnemy.EnteredAnyEnemy
                .Take(1)
                .Subscribe(this, (b, state) => { _ = state.PlayAttackAnimationAsync(); })
                .AddTo(this);
        }
    }

    private protected override void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
        _lifestealAmount = 0;
        _lifeStealRate = 0f;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                case Lifesteal lifesteal:
                {
                    _lifestealAmount += lifesteal.Amount;
                    _lifeStealRate += lifesteal.Rate;
                    break;
                }
            }
        }
    }

    private protected override void OnStartAttack()
    {
        StaticDamage.Disabled = true;
        StaticDamage.Damage = Damage;
        StaticDamage.Knockback = Knockback;
    }

    private async ValueTask PlayAttackAnimationAsync()
    {
        // A. 包丁のひきはじめ, 通常時よりも感度を下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();

        // A. 攻撃前のナイフを構えるアニメーション
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.2d)
            .SetEase(Tween.EaseType.InOut);

        // B. ナイフを前に突き出すアニメーション, この段階でエイム感度をほぼ 0 にする
        t.TweenCallback(Callable.From(() => { AimToNearEnemy.RotateSensitivity = 0.001f; }));
        for (var i = 0; i < PushCount; i++)
        {
            RegisterPushAnimation(t, sprite);
        }

        // すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // Tween の終了を待つ
        await ToSignal(t, Tween.SignalName.Finished);

        // 通常の感度に戻す
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
        RestartCoolDown();
    }

    private void RegisterPushAnimation(Tween tween, Node2D sprite)
    {
        // 突き刺しアニメーション
        var distance = (float)_pushDistance;
        var totalDulation = _pushDuration / 60f;
        var backDulation = 0.08f;
        var pushDulation = Mathf.Max(totalDulation - 0.08f, 0.01f);

        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = false; })); // ダメージを有効化
        tween.TweenProperty(sprite, "position", new Vector2(distance, 0), pushDulation)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);

        // 手元に戻すアニメーション
        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = true; })); // ダメージを無効化
        tween.TweenProperty(sprite, "position", new Vector2(3, 0), backDulation);
    }
}