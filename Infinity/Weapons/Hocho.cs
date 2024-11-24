using System;
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
    /// 突き刺し回数
    /// </summary>
    [Export(PropertyHint.Range, "1,10")]
    private int PushCount
    {
        get;
        set => field = Mathf.Clamp(value, 1, 10);
    } = 1;

    // ==== Aim Settings ====

    [ExportGroup("Aim Settings")]
    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    private float _minRange;

    [Export(PropertyHint.Range, "0,200,,or_greater,suffix:px")]
    private float _maxRange = 100f;

    /// <summary>
    /// 敵を狙う速度の感度 (0 ~ 1), 1 で最速, 0 で全然狙えない
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    private float _rotateSensitivity = 0.3f;

    // ==== Animation Settings ====

    // 攻撃前の構えるアニメーションの距離
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    private uint _preAttackDistance = 10u;

    // 攻撃前の構えるアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _preAttackDuration = 4;

    // 突き刺しアニメーションの距離
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40u;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushDuration = 10;

    private AimEntity? _aimEntity;
    private IDisposable? _tweenPlayingDisposable;
    private IDisposable? _waitEnterEntityDisposable;

    private AimEntity AimEntity
    {
        get
        {
            if (_aimEntity is not null)
            {
                return _aimEntity;
            }

            // 初回アクセスのキャッシュ
            var a = this.FindFirstChild<AimEntity>();
            _aimEntity = a ?? throw new ApplicationException($"Failed to find AimEntity node in {Name}");
            return _aimEntity;
        }
    }

    private protected StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override void _Ready()
    {
        AimEntity.Mode = AimEntity.TargetMode.NearestEntity;
        AimEntity.MinRange = _minRange;
        AimEntity.MaxRange = _maxRange;
        AimEntity.RotateSensitivity = _rotateSensitivity;
    }

    public override void _ExitTree()
    {
        _tweenPlayingDisposable?.Dispose();
        _waitEnterEntityDisposable?.Dispose();
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        // クールダウン終了時に範囲内に敵がいれば攻撃アニメーションを実行
        if (AimEntity.IsAiming)
        {
            PlayAttackAnimation();
        }
        // そうでない場合は敵が入ってくるのを待つ
        else
        {
            if (_waitEnterEntityDisposable is not null)
            {
                throw new InvalidProgramException($"_waitEnterEntityDisposable is already exist in {Name}");
            }

            // 範囲内に敵が入ってきたら攻撃アニメーションを実行
            // 確実に当ててほしいので, 5度以内になるまで待機する
            // 待機中に目標を見失った場合はまた敵の侵入を待機する 
            // Note: AwaitOperation.Drop により, 待機中に来る通知は無視される
            _waitEnterEntityDisposable = AimEntity.EnteredAnyEntity
                .SubscribeAwait(async (_, token) =>
                {
                    // 狙いを定めるまで待機する
                    var failed = false;
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (!AimEntity.IsAiming)
                        {
                            failed = true;
                            break;
                        }

                        await this.NextPhysicsFrameAsync();
                        var diff = Mathf.Abs(AimEntity.AngleDiff);
                        if (diff < 0.0872665f) // 5°
                        {
                            break;
                        }
                    }

                    if (!failed)
                    {
                        _waitEnterEntityDisposable?.Dispose();
                        _waitEnterEntityDisposable = null;
                        PlayAttackAnimation();
                    }
                }, AwaitOperation.Drop);
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
        if (_tweenPlayingDisposable is not null)
        {
            throw new InvalidProgramException($"Tween is already playing in {Name}");
        }

        // A. 包丁のひきはじめ, 通常時よりも感度を下げる
        AimEntity.RotateSensitivity = _rotateSensitivity * 0.5f;

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
            t.TweenCallback(Callable.From(() => { AimEntity.RotateSensitivity = 0.001f; }));
            for (var i = 0; i < PushCount; i++)
            {
                RegisterPushAnimation(t, sprite);
            }
        }

        // C. すぐに他の敵を狙わないようなアニメの遊び
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // D. Tween の終了後の処理
        _tweenPlayingDisposable = t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) =>
            {
                // 通常の感度に戻す
                state.AimEntity.RotateSensitivity = _rotateSensitivity;
                // クールダウンを再開する
                state.RestartCoolDown();

                // Disposable を開放
                state._tweenPlayingDisposable?.Dispose();
                state._tweenPlayingDisposable = null;
            });
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