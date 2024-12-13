using Godot;

namespace fms.Enemy;

/// <summary>
/// 突進を繰り返す敵
/// </summary>
public partial class Charger : EntityEnemy
{
    /// <summary>
    /// 突進後のクールダウン
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1,suffix:frames")]
    private uint _chargeFrame = 60;

    [Export(PropertyHint.Range, "1,9999,1,suffix:frames")]
    private uint _maxAttackFrame = 120;


    private ChargerState _chargerState = ChargerState.Idle;
    private uint _frameTimer;

    public override void _Ready()
    {
        // Subscribe and start FrameTimer
        var timer = new FrameTimer();
        AddChild(timer);
        timer.WaitFrame = _chargeFrame;
    }

    public override void _PhysicsProcess(double delta)
    {
        _frameTimer++;

        if (_chargerState == ChargerState.Idle)
        {
            // ノックバック中であれば何もしない
            if (IsDead || Knockbacking)
            {
                return;
            }

            // Animator 用に次の突進方向を保存しておく (ユーザーに狙う方向を開示)
            var v = _playerNode!.GlobalPosition - GlobalPosition;
            TargetVelocity = v.Normalized();

            // 待機が完了したら突進を開始する
            if (_frameTimer > _chargeFrame)
            {
                _chargerState = ChargerState.AttackToPlayer;
                _frameTimer = 0;

                // Note: Impulse は 1/mass がかかるので, Mass を事前にかけてスケーリングしておく (一様に px/s の Speed で調整したいため)
                var impulse = TargetVelocity * State.MoveSpeed.CurrentValue * Mass;
                ApplyCentralImpulse(impulse);
            }
        }
        else if (_chargerState == ChargerState.AttackToPlayer)
        {
            if (IsDead)
            {
                return;
            }

            var direction = _playerNode.GlobalPosition - GlobalPosition;

            // プレイヤーがめちゃくちゃ近くに来たらダメージを与えて突進終了
            if (direction.LengthSquared() < 300)
            {
                ((IEntity)_playerNode).ApplayDamage(BaseDamage, this, this, CauserPath);
                ToIdle();
                return;
            }

            // 突進方向がそれていたら突進終了する
            // 進行方向とプレイヤーとの角度が 90 度以上になったら突進終了
            var dot = direction.Normalized().Dot(LinearVelocity.Normalized());
            if (dot < 0)
            {
                ToIdle();
                return;
            }

            // 最大攻撃時間が過ぎたら突進終了
            if (_frameTimer > _maxAttackFrame)
            {
                ToIdle();
            }
        }
    }

    public override void ApplyKnockback(in Vector2 impulse)
    {
        // 突進中はノックバックを受け付けない
        if (_chargerState == ChargerState.AttackToPlayer)
        {
            return;
        }

        base.ApplyKnockback(in impulse);
    }

    private void ToIdle()
    {
        _chargerState = ChargerState.Idle;
        _frameTimer = 0;
        TargetVelocity = Vector2.Zero;
        LinearVelocity = Vector2.Zero;
    }

    private enum ChargerState
    {
        Idle,
        AttackToPlayer
    }
}
