using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
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

    private AimToNearEnemy _aimToNearEnemy = null!;

    private StaticDamage StaticDamage => GetNode<StaticDamage>("%StaticDamage");

    public override void _Ready()
    {
        _aimToNearEnemy = GetNode<AimToNearEnemy>("AimToNearEnemy");
        _aimToNearEnemy.SearchRadius = _maxRange;
        _aimToNearEnemy.RotateSensitivity = _rotateSensitivity;
    }

    private protected override void OnCoolDownComplete(uint level)
    {
        if (!_aimToNearEnemy.IsAiming)
        {
            return;
        }

        // 包丁のひきはじめ, 通常時よりも感度を下げる
        _aimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        // Sprite に対して Tween で突き刺しアニメーション
        // ToDo: 固定長のアニメーションなので, BaseCoolDown のほうが早くなるとおかしくなる 
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(() =>
        {
            // 突き刺し時は回転しないようにする
            _aimToNearEnemy.RotateSensitivity = 0.01f;
            // ダメージを有効化
            StaticDamage.Disabled = false;
        }));
        t.TweenProperty(sprite, "position", new Vector2(57, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
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
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state._aimToNearEnemy.RotateSensitivity = _rotateSensitivity; })
            .AddTo(this);
    }

    private protected override void OnStartAttack()
    {
        StaticDamage.Disabled = true;
        StaticDamage.Damage = BaseDamage;
        StaticDamage.Knockback = Knockback;
    }
}