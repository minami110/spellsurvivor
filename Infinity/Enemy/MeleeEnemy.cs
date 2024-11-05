using Godot;
using R3;

namespace fms.Enemy;

/// <summary>
/// 近距離 (接触) ダメージを与える敵のベースクラス
/// </summary>
public partial class MeleeEnemy : EnemyBase
{
    /// <summary>
    ///     プレイヤーと重なっている時攻撃を発生させるクールダウン
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    private uint _coolDownFrame = 20;

    public override void _Ready()
    {
        // Subscribe and start FrameTimer
        var timer = new FrameTimer();
        AddChild(timer);
        var d1 = timer.TimeOut.Subscribe(_ => Attack()).AddTo(this);
        timer.WaitFrame = _coolDownFrame;
        timer.Start();

        Disposable.Combine(_state, d1).AddTo(this);
    }

    public override void _PhysicsProcess(double _)
    {
        var delta = _playerNode!.GlobalPosition - GlobalPosition;
        // 20 px 以内に近づいたら移動を停止する
        if (delta.LengthSquared() < 400)
        {
            LinearVelocity = Vector2.Zero;
            return;
        }

        var direction = delta.Normalized();
        var force = direction * _state.MoveSpeed.CurrentValue;
        LinearVelocity = force;
    }


    private protected override void Attack()
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
                entity.ApplayDamage(_power, this, this);
            }
        }
    }
}