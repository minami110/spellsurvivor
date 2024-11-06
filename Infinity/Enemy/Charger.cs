using Godot;

namespace fms.Enemy;

/// <summary>
/// 突進を繰り返す敵
/// </summary>
public partial class Charger : EnemyBase
{
    private enum ChargerState
    {
        Idle,
        AttackToPlayer,
    }

    /// <summary>
    ///   突進後のクールダウン
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    private uint _chargeFrame = 60;

    [Export(PropertyHint.Range, "1,9999,1")]
    private uint _maxAttackFrame = 120;


    private ChargerState _chargerState = ChargerState.Idle;
    private uint _frameTimer = 0u;

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
            // 待機が完了したら突進を開始する
            if (_frameTimer > _chargeFrame)
            {
                _chargerState = ChargerState.AttackToPlayer;
                _frameTimer = 0;
                var v = _playerNode!.GlobalPosition - GlobalPosition;
                var force = v.Normalized() * _state.MoveSpeed.CurrentValue * 60;
                ApplyCentralImpulse(force);
            }
        }
        else if (_chargerState == ChargerState.AttackToPlayer)
        {
            var direction = _playerNode.GlobalPosition - GlobalPosition;

            // プレイヤーがめちゃくちゃ近くに来たらダメージを与えて突進終了
            if (direction.LengthSquared() < 300)
            {
                ((IEntity)_playerNode).ApplayDamage(_power, this, this);
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
                return;
            }
        }
    }

    private void ToIdle()
    {
        _chargerState = ChargerState.Idle;
        _frameTimer = 0;
        LinearVelocity = Vector2.Zero;
    }


}