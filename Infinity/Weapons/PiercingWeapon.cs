using System;
using fms.Projectile;
using Godot;
using Godot.Collections;
using R3;

namespace fms.Weapon;

/// <summary>
/// 包丁のような突き刺しアニメーションを持つ近接武器のベースクラス
/// </summary>
public partial class PiercingWeapon : WeaponBase
{
    // ==== Damage Settings ====
    // 突き刺し回数
    [ExportGroup("Damage Settings")]
    [Export(PropertyHint.Range, "1,10,,suffix:push")]
    private int PushCount
    {
        get;
        set => field = Mathf.Clamp(value, 1, 10);
    } = 1;

    // 攻撃前の構えるアニメーションの距離
    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    private uint _preAttackDistance = 10u;

    // 突き刺しアニメーションの距離
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40u;

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
    // 攻撃前の構えるアニメーションのフレーム数
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _beginAttackDuration = 4u;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushFrontDuration = 12u;

    // 突き刺したあと手前に戻すアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushBackDuration = 4u;

    // すべての攻撃終了後の待機フレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _endAttackDuration = 20u;

    private AimEntity? _aimEntity;
    private AimEntityEnterTargetWaiter? _aimEntityEnterTargetWaiter;
    private IDisposable? _tweenPlayingDisposable;

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
            _aimEntityEnterTargetWaiter = new AimEntityEnterTargetWaiter(_aimEntity);
            return _aimEntity;
        }
    }

    private protected StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override uint AnimationTime
    {
        get
        {
            var preAttack = _beginAttackDuration; // 攻撃前の構えるアニメーション
            var push = (uint)((_pushFrontDuration + _pushBackDuration) * PushCount); // 突き刺しアニメーション時間 * 突き刺し回数
            var endAttack = _endAttackDuration; // 通常の感度に戻すアニメーション
            return preAttack + push + endAttack;
        }
    }

    public override void _Ready()
    {
        // "ダメージを与えたとき" の処理用にイベントを登録
        StaticDamage.CauserPath = CauserPath;
        StaticDamage.Hit.Subscribe(this, (payload, state) => { state.OnHitAnyEntity(payload); }).AddTo(this);
    }

    public override void _ExitTree()
    {
        _tweenPlayingDisposable?.Dispose();
        _aimEntityEnterTargetWaiter?.Dispose();
    }

    internal override string GetDescriptionForShop()
    {
        var desc = base.GetDescriptionForShop() + "\n";
        desc += $"Push Count: {PushCount}\n";
        desc += $"Range: {_minRange} ~ {_maxRange} px";
        return desc;
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        _aimEntityEnterTargetWaiter?.Start(5f, PlayAttackAnimation);
    }

    private protected override void OnStartAttack(uint level)
    {
        AimEntity.Mode = AimEntity.TargetMode.NearestEntity;
        AimEntity.MinRange = _minRange;
        AimEntity.MaxRange = _maxRange;
        AimEntity.RotateSensitivity = _rotateSensitivity;

        StaticDamage.Disabled = true;
    }

    private protected override void OnStopAttack()
    {
        _aimEntityEnterTargetWaiter?.Cancel();
    }

    private void OnHitAnyEntity(Dictionary info)
    {
        // Lifesteal の処理
        var lifestealRate = State.LifestealRate.CurrentValue;
        if (lifestealRate > 0f)
        {
            // ToDo: ライフスティールの対象かどうか?
            // Player なら Enemy にあたったとき みたいな確認
            // var hitEntity = (IEntity)(Node2D)payload["Entity"];

            // ライフスティールの確率計算
            var chance = Math.Clamp(lifestealRate, 0f, 1f);
            if (GD.Randf() >= chance)
            {
                return;
            }

            // OwnedEntity (Player) を回復する
            var payload = new Dictionary
            {
                { "EntityType", "Player" }
            };
            OwnedEntity.ApplayDamage(-5.0f, OwnedEntity, this, CauserPath);
        }
    }

    private void PlayAttackAnimation()
    {
        var animationSpeedRate = State.AttackSpeed.Rate;

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
            var dul = _beginAttackDuration / 60d / animationSpeedRate;
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
        {
            var dul = _endAttackDuration / 60d / animationSpeedRate;
            t.TweenProperty(sprite, "position", new Vector2(0, 0), dul);
        }

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
        var rate = State.AttackSpeed.Rate;

        // 突き刺しアニメーション
        var dist = (float)_pushDistance;

        // ToDo: ゲーム側から武器速度 <see cref="SpeedRate"/> が変更された場合, StaticDamage では
        // 対応しきれないような速度になるので, 一定値以上であれば Static Damage の On / Off ではなく
        // RectAreaProjectile を生成すること 
        tween.TweenCallback(Callable.From(() =>
        {
            StaticDamage.Damage = State.Damage.CurrentValue;
            StaticDamage.Knockback = State.Knockback.CurrentValue;
            StaticDamage.Disabled = false;
        })); // ダメージを有効化

        // 突き刺し
        {
            var dul = _pushFrontDuration / 60d / rate;
            tween.TweenProperty(sprite, "position", new Vector2(dist, 0), dul)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.InOut);
        }

        tween.TweenCallback(Callable.From(() => { StaticDamage.Disabled = true; })); // ダメージを無効化

        // 手元に戻すアニメーション
        {
            var dul = _pushBackDuration / 60d / rate;
            tween.TweenProperty(sprite, "position", new Vector2(3, 0), dul);
        }
    }
}