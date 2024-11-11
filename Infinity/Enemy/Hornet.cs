using System;
using fms.Projectile;
using fms.Weapon;
using Godot;
using R3;

namespace fms.Enemy;

/// <summary>
/// 遠距離からプレイヤーに射撃を行う敵
/// </summary>
public partial class Hornet : EnemyBase
{
    [Export(PropertyHint.Range, "0,10000,1")]
    private int _minAttackDistance = 180;

    [Export(PropertyHint.Range, "0,10000,1")]
    private int _maxAttackDistance = 400;

    [Export(PropertyHint.Range, "1,9999,1")]
    public uint _baseCoolDownFrame = 40u;

    [Export]
    private PackedScene _projectile = null!;

    private MovementState _moveState = MovementState.FollowPlayer;

    private WeaponBase _weapon = null!;


    public override void _EnterTree()
    {
        // Weapon が存在していなかったら作成する
        var weapon = GetNodeOrNull<WeaponBase>("Weapon");
        if (weapon == null)
        {
            weapon = new WeaponBase();
            AddChild(weapon);
        }

        _weapon = weapon;
    }

    public override void _Ready()
    {
        // Weapon 内部にある FrameTimer の初期化/購読を行う
        // Note: Weapon 側の実装で必ずこの名前で生成されています
        var frameTimer = _weapon.GetNode<FrameTimer>("FrameTimer");
        frameTimer.TimeOut.Subscribe(this, (_, state) => { state.Attack(); })
            .AddTo(this);

        frameTimer.WaitFrame = _baseCoolDownFrame;
    }

    public override void _PhysicsProcess(double _)
    {
        var frameTimer = _weapon.GetNode<FrameTimer>("FrameTimer");

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
                frameTimer.Start();
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
                frameTimer.Stop();
            }
            // プレイヤーが 最大距離以上離れたら追跡モードに移行する
            else if (lengthSqr >= _maxAttackDistance * _maxAttackDistance)
            {
                _moveState = MovementState.FollowPlayer;
                frameTimer.Stop();
            }
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // ノックバック中であれば何もしない
        if (Knockbacking)
        {
            return;
        }

        var delta = _playerNode!.GlobalPosition - GlobalPosition;

        // 追跡モードのときはプレイヤーに近づいていく
        if (_moveState == MovementState.FollowPlayer)
        {
            var direction = delta.Normalized();
            var vel = direction * State.MoveSpeed.CurrentValue;
            state.LinearVelocity = vel;
        }
        else if (_moveState == MovementState.AwayPlayer)
        {
            var direction = -delta.Normalized();
            var vel = direction * State.MoveSpeed.CurrentValue;
            state.LinearVelocity = vel;
        }
        else if (_moveState == MovementState.AttackPlayer)
        {
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
            prj.Speed = 180;
            prj.LifeFrame = 300;
            prj.PenetrateEnemy = false;
            prj.PenetrateWall = false;
        }

        // プレイヤーに向けて発射する
        var direction = (_playerNode!.GlobalPosition - GlobalPosition).Normalized();
        var pos = GlobalPosition;

        // 武器から発射する
        _weapon.AddProjectile(prj, pos, direction);
    }

    private enum MovementState
    {
        FollowPlayer,
        AwayPlayer,
        AttackPlayer
    }
}