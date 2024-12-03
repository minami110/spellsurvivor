using System;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Enemy;

/// <summary>
/// 遠距離からプレイヤーに射撃を行う敵
/// </summary>
public partial class Hornet : EntityEnemy
{
    [Export(PropertyHint.Range, "0,10000,1")]
    private int _minAttackDistance = 180;

    [Export(PropertyHint.Range, "0,10000,1")]
    private int _maxAttackDistance = 400;

    [Export]
    private PackedScene _projectile = null!;

    private MovementState _moveState = MovementState.FollowPlayer;

    private WeaponBase _weapon = null!;

    public override void _EnterTree()
    {
        // Weapon が存在していなかったら作成する
        var weapon = GetNodeOrNull<WeaponBase>("Weapon");
        _weapon = weapon ?? throw new ApplicationException("Weapon が存在しません");
    }

    public override void _Ready()
    {
        // Weapon 内部にある FrameTimer の初期化/購読を行う
        var frameTimer = _weapon.FindFirstChild<FrameTimer>();
        frameTimer!.TimeOut.Subscribe(this, (_, state) => { state.Attack(); })
            .AddTo(this);
    }

    public override void _PhysicsProcess(double _)
    {
        // プレイヤーとの距離を計算する
        var delta = _playerNode!.GlobalPosition - GlobalPosition;
        var lengthSqr = delta.LengthSquared();
        var softLength = 25; // 移動ばっかにならないようにするためのソフトな距離

        // 追跡モードのとき
        if (_moveState == MovementState.FollowPlayer)
        {
            // 最大距離 - ソフト距離 以内に来たら攻撃モードに移行する
            if (lengthSqr <= Math.Pow(_maxAttackDistance - softLength, 2))
            {
                _moveState = MovementState.AttackPlayer;

                _weapon.StartAttack();
            }
        }
        // 逃走モードの時
        else if (_moveState == MovementState.AwayPlayer)
        {
            // 最大距離 - ソフト距離 まで離れる
            if (lengthSqr >= Math.Pow(_maxAttackDistance - softLength, 2))
            {
                _moveState = MovementState.FollowPlayer;
            }
        }
        else if (_moveState == MovementState.AttackPlayer)
        {
            // プレイヤーが  最小距離以内に来たら逃走モードに移行する
            if (lengthSqr <= _minAttackDistance * _minAttackDistance)
            {
                _moveState = MovementState.AwayPlayer;
                _weapon.StopAttack();
            }
            // プレイヤーが 最大距離以上離れたら追跡モードに移行する
            else if (lengthSqr >= _maxAttackDistance * _maxAttackDistance)
            {
                _moveState = MovementState.FollowPlayer;
                _weapon.StopAttack();
            }
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // ノックバック中であれば何もしない
        if (IsDead || Knockbacking)
        {
            return;
        }

        var delta = _playerNode!.GlobalPosition - GlobalPosition;

        // 追跡モードのときはプレイヤーに近づいていく
        if (_moveState == MovementState.FollowPlayer)
        {
            TargetVelocity = delta.Normalized();
            var vel = TargetVelocity * State.MoveSpeed.CurrentValue;
            state.LinearVelocity = vel;
        }
        else if (_moveState == MovementState.AwayPlayer)
        {
            TargetVelocity = -delta.Normalized();
            var vel = TargetVelocity * State.MoveSpeed.CurrentValue;
            state.LinearVelocity = vel;
        }
        else if (_moveState == MovementState.AttackPlayer)
        {
            TargetVelocity = Vector2.Zero;
            state.LinearVelocity = Vector2.Zero;
        }
    }

    private void Attack()
    {
        // 弾生成
        var prj = _projectile.Instantiate<BulletProjectile>();
        {
            // ToDo: 発射する弾の仕様 (仮実装)
            // プレイヤー / 壁 に当たるようにする
            prj.CollisionMask = Constant.LAYER_PLAYER | Constant.LAYER_WALL;
            prj.Damage = BaseDamage;
            prj.LifeFrame = 300;
            prj.PenetrateEnemy = false;
            prj.PenetrateWall = false;
        }

        // プレイヤーに向けて発射する
        var direction = (_playerNode.GlobalPosition - GlobalPosition).Normalized();
        prj.ConstantForce = direction * 180;

        var pos = GlobalPosition;

        // 武器から発射する
        _weapon.AddProjectile(prj, pos);
    }

    private enum MovementState
    {
        FollowPlayer,
        AwayPlayer,
        AttackPlayer
    }
}