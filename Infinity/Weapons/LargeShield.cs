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

    // 攻撃前の盾を構える距離
    [ExportGroup("Animation")]
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _preAttackDistance = 20;

    // 攻撃前の盾を構えるフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _preAttackDuration = 4;

    // 突き刺しアニメーションの距離
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private uint _pushDistance = 40;

    // 突き刺しアニメーションのフレーム数
    [Export(PropertyHint.Range, "0,100,1,suffix:frames")]
    private uint _pushDuration = 10;

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

        // A. 盾を手前に引くアニメーション
        var distA = _preAttackDistance * -1f;
        var durA = _preAttackDuration / 60d;
        t.TweenProperty(sprite, "position", new Vector2(distA, 0), durA)
            .SetEase(Tween.EaseType.InOut);

        // B. 盾を前に突き出すアニメーション, この段階でエイム感度をほぼ 0 にする
        var distB = _pushDistance;
        var durB = _pushDuration / 60d;
        t.TweenCallback(Callable.From(() => { AimToNearEnemy.RotateSensitivity = 0.01f; }));
        t.TweenProperty(sprite, "position", new Vector2(distB, 0), durB)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);

        // C. 攻撃判定を生成して盾を戻す (突き出しアニメーション終了時に発生)
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
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.LifeFrame = 10u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えたら消滅する
            prj.Radius = _damageRadius;
            prj.AngleLimit = _angleLimit;
            prj.Offset = new Vector2(0, 0); // この武器の中心からのオフセット
        }

        AddProjectile(prj, GlobalPosition, AimToNearEnemy.GlobalRotation);
    }
}