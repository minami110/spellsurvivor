using System.Threading.Tasks;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class LargeShield : WeaponBase
{
    /// <summary>
    /// 生成するダメージエリアの半径
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _damageRadius = 200u;

    /// <summary>
    /// 生成する円弧の角度
    /// </summary>
    [Export(PropertyHint.Range, "0,360,1,suffix:°")]
    private float _angleLimit = 90f;

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
        // 近くに敵がいない場合は AimToNearEnemy の次のヒットイベントを購読する
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

    private async ValueTask PlayAttackAnimationAsync()
    {
        // 攻撃アニメーションを再生する
        // この段階でエイム速度を少し下げる
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity * 0.5f;

        var sprite = GetNode<Node2D>("%Sprite");
        var t = CreateTween();

        // 盾を手前に引くアニメーション
        t.TweenProperty(sprite, "position", new Vector2(-20, 0), 0.3d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);

        // 盾を前に突き出すアニメーション, この段階でエイム感度をほぼ 0 にする
        t.TweenCallback(Callable.From(() => { AimToNearEnemy.RotateSensitivity = 0.01f; }));
        t.TweenProperty(sprite, "position", new Vector2(10, 0), 0.03d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);

        // 攻撃判定を生成して盾を戻す
        t.TweenCallback(Callable.From(SpawnDamage));
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // Tween の再生を待つ
        await ToSignal(t, Tween.SignalName.Finished);
        AimToNearEnemy.RotateSensitivity = _rotateSensitivity; // エイム感度を戻す
        RestartCoolDown(); // クールダウンを再開
    }

    private void SpawnDamage()
    {
        var prj = new ArcAreaProjectile();
        {
            prj.Damage = Damage;
            prj.Knockback = Knockback;
            prj.LifeFrame = 30u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えたら消滅する
            prj.Radius = _damageRadius;
            prj.AngleLimit = _angleLimit;
            prj.Offset = new Vector2(0, 0); // この武器の中心からのオフセット
        }

        AddProjectile(prj, GlobalPosition, AimToNearEnemy.GlobalRotation);
    }
}