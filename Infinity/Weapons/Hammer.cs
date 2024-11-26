using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class Hammer : WeaponBase
{
    /// <summary>
    /// 生成するダメージエリアの半径
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _damageRadius = 100u;

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

    private void PlayAttackAnimation()
    {
        var sprite = GetNode<Node2D>("%Sprite");
        var t = CreateTween();

        // A. ハンマーを振り上げるアニメーション
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;
        t.TweenProperty(sprite, "position", new Vector2(0, -20), 0.3d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);

        // B. ハンマーを振り下ろすアニメーション
        t.TweenCallback(Callable.From(() => { AimToNearEnemy.RotateSensitivity = 0.001f; }));
        t.TweenProperty(sprite, "position", new Vector2(0, 10), 0.03d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);

        // C. ハンマーを元の位置に戻すアニメーション, 振り下ろしのタイミングでダメージ発生
        t.TweenCallback(Callable.From(SpawnDamage));
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // D. アニメーション終了後の処理
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) =>
            {
                state.AimToNearEnemy.RotateSensitivity = _rotateSensitivity;
                state.RestartCoolDown();
            })
            .AddTo(this);
    }

    private void SpawnDamage()
    {
        // アニメーションに合うようにエリア攻撃の弾を生成する
        var prj = new CircleAreaProjectile();
        {
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.LifeFrame = 10u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えたら消滅する
            prj.Radius = _damageRadius;
            prj.Offset = new Vector2(0, 0); // この武器の中心からのオフセット
        }

        AddProjectile(prj, GlobalPosition);
    }
}