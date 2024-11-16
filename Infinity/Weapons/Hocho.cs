using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class Hocho : WeaponBase
{
    /// <summary>
    /// 生成するダメージエリアのサイズ
    /// </summary>
    [Export]
    private Vector2 _damageSize = new(80, 10);

    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 100f;

    private AimToNearEnemy _aimToNearEnemy = null!;

    public override void _Ready()
    {
        _aimToNearEnemy = GetNode<AimToNearEnemy>("AimToNearEnemy");
        _aimToNearEnemy.SearchRadius = _maxRange;
    }

    private protected override void OnCoolDownComplete(uint level)
    {
        if (!_aimToNearEnemy.IsAiming)
        {
            return;
        }

        // ToDo: BaseCoolDown = 30f 決め打ちのアニメ を再生している
        var sprite = GetNode<Node2D>("%SpriteRoot");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(-12, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(Attack));
        t.TweenProperty(sprite, "position", new Vector2(57, 0), 0.2d)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.08d);

        // 再生終了したら AimToNearEnemy を再開する
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state._aimToNearEnemy.SetPhysicsProcess(true); })
            .AddTo(this);
    }

    // アニメーションの最初のタメ が終わったタイミングで呼ばれるコールバック
    private void Attack()
    {
        // この時点で狙う方向が確定するので,
        // 敵を殺したときに変な回転をしないように physics_process を止める 
        _aimToNearEnemy.SetPhysicsProcess(false);

        // 現在狙っている敵を取得
        var enemy = _aimToNearEnemy.NearestEnemy;
        if (enemy is null)
        {
            // ToDo: 狙っている間に敵が殺されたパターンの処理を考えていない
            return;
        }

        // アニメーションに合うようにエリア攻撃の弾を生成する
        var prj = new RectAreaProjectile();
        {
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = 30u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えたら消滅する
            prj.Size = _damageSize;
            prj.Offset = new Vector2(_damageSize.X / 2f, 0f); // 原点に左辺が重なるような Offset を設定
        }

        // 敵の方向を向くような rotation を計算する
        var dir = enemy.GlobalPosition - GlobalPosition;
        var angle = dir.Angle();

        // 自分の位置から angle 方向に ちょっと伸ばした位置を計算してダメージを発生させる
        // Note: プレイ間確かめながらスポーン位置のピクセル数は調整する
        var pos = GlobalPosition + dir.Normalized() * 20;

        AddProjectile(prj, pos, angle);
    }
}