using Godot;
using R3;

namespace fms.Enemy;

/// <summary>
/// 近距離 (接触) ダメージを与える敵のベースクラス
/// </summary>
public partial class MeleeEnemy : EnemyBase
{
    /// <summary>
    /// プレイヤーと重なっている時攻撃を発生させるクールダウン
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1,suffix:frames")]
    private uint _coolDownFrame = 20;

    public override void _Ready()
    {
        // プレイヤーを攻撃するためのタイマーを設定する
        var timer = new FrameTimer();
        AddChild(timer);
        var d1 = timer.TimeOut.Subscribe(_ => Attack()).AddTo(this);
        timer.WaitFrame = _coolDownFrame;
        timer.Start();
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // ノックバック中であれば何もしない
        if (IsDead || Knockbacking)
        {
            return;
        }

        // プレイヤーとの距離が 20px 以内に近づいたら移動を停止する
        var delta = _playerNode!.GlobalPosition - GlobalPosition;
        if (delta.LengthSquared() <= 400)
        {
            state.LinearVelocity = Vector2.Zero;
            return;
        }

        // プレイヤーに指定した速度で近づく Velocity を設定する
        TargetVelocity = delta.Normalized(); // Note: Animator 用にキャッシュしておく
        var vel = TargetVelocity * State.MoveSpeed.CurrentValue;
        state.LinearVelocity = vel;
    }

    private protected virtual void Attack()
    {
        var damageArea = GetNode<Area2D>("DamageArea");
        var overlappingBodies = damageArea.GetOverlappingBodies();
        if (overlappingBodies.Count <= 0)
        {
            return;
        }

        foreach (var node in overlappingBodies)
        {
            if (node is IEntity entity)
            {
                entity.ApplayDamage(BaseDamage, this, this);
            }
        }
    }
}