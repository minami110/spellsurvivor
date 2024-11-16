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

        // Note: 30frame かかるアニメーションを再生しているので, CoolDown が追いついちゃったら変なことなります
        var sprite = GetNode<Node2D>("%Sprite");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(0, -20), 0.3d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "position", new Vector2(0, 10), 0.03d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(Attack));
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // 再生終了したら AimToNearEnemy を再開する
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state._aimToNearEnemy.SetPhysicsProcess(true); })
            .AddTo(this);
    }

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
        var prj = new CircleAreaProjectile();
        {
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = 30u; // Note: 一発シバいたら終わりの当たり判定なので寿命は短めな雑な値
            prj.DamageEveryXFrames = 0u; // 一度ダメージを与えたら消滅する
            prj.Radius = _damageRadius;
            prj.Offset = new Vector2(0, 0); // この武器の中心からのオフセット
        }

        AddProjectile(prj, GlobalPosition);
    }
}