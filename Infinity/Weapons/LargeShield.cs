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

    private AimToNearEnemy _aimToNearEnemy = null!;

    public override void _Ready()
    {
        _aimToNearEnemy = GetNode<AimToNearEnemy>("AimToNearEnemy");
        _aimToNearEnemy.SearchRadius = _maxRange;
    }

    private protected override async void OnCoolDownCompleted(uint level)
    {
        // 近くに敵がいない場合はクールダウンを停止して
        if (!_aimToNearEnemy.IsAiming)
        {
            return;
        }

        // Note: 30frame かかるアニメーションを再生しているので, CoolDown が追いついちゃったら変なことなります
        var sprite = GetNode<Node2D>("%Sprite");
        var t = CreateTween();
        t.TweenProperty(sprite, "position", new Vector2(-20, 0), 0.3d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenProperty(sprite, "position", new Vector2(10, 0), 0.03d)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        t.TweenCallback(Callable.From(Attack));
        t.TweenProperty(sprite, "position", new Vector2(0, 0), 0.2d);

        // 再生終了したら AimToNearEnemy を再開する
        t.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => { state._aimToNearEnemy.SetPhysicsProcess(true); })
            .AddTo(this);

        await ToSignal(t, Tween.SignalName.Finished);

        RestartCoolDown();
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

        // 敵の方向を向くような rotation を計算する
        var dir = enemy.GlobalPosition - GlobalPosition;
        var angle = dir.Angle();

        AddProjectile(prj, GlobalPosition, angle);
    }
}