using System;
using System.Threading.Tasks;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class AssaultRifle : WeaponBase
{
    [ExportGroup("Damage Settings")]
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 200f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120u;

    [Export(PropertyHint.Flags, "Enemy:1,Wall:2")]
    private int _penetrate;

    [Export(PropertyHint.Range, "1,20,,or_greater")]
    private uint _magazineSize = 1u;

    [Export(PropertyHint.Range, "0,60,,or_greater,suffix:frames")]
    private uint _fireRate = 5u;

    [Export]
    private Node2D? _muzzle;

    // ==== Aim Settings ====
    [ExportGroup("Aim Settings")]
    [Export]
    private AimEntity.TargetMode _targetMode = AimEntity.TargetMode.NearestEntity;

    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    private float _minRange;

    [Export(PropertyHint.Range, "0,200,,or_greater,suffix:px")]
    private float _maxRange = 100f;

    /// <summary>
    /// 敵を狙う速度の感度 (0 ~ 1), 1 で最速, 0 で全然狙えない
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    private float _rotateSensitivity = 0.3f;

    [Export(PropertyHint.Range, "0,1")]
    private float _rotateSensitivityAttacking = 0.05f;

    private AimEntity? _aimEntity;
    private AimEntityEnterTargetWaiter? _enterTargetWaiter;

    public override uint AnimationTime => (_magazineSize - 1) * _fireRate;

    private AimEntity AimEntity
    {
        get
        {
            if (_aimEntity is not null)
            {
                return _aimEntity;
            }

            // 初回アクセスのキャッシュ
            var a = this.FindFirstChild<AimEntity>();
            _aimEntity = a ?? throw new ApplicationException($"Failed to find AimEntity node in {Name}");
            _enterTargetWaiter = new AimEntityEnterTargetWaiter(_aimEntity);
            return _aimEntity;
        }
    }

    private StateType StateMachine
    {
        get;
        set
        {
            // 状態遷移の制約
            switch (field)
            {
                // InShop => Reloading
                case StateType.InShop when value == StateType.Reloading:
                // Reloading => Ready
                case StateType.Reloading when value == StateType.Ready:
                // Ready => Attacking
                case StateType.Ready when value == StateType.Attacking:
                // Attacking => Reloading
                case StateType.Attacking when value == StateType.Reloading:
                // Reloading => InShop
                case StateType.Reloading when value == StateType.InShop:
                // Ready => InShop
                case StateType.Ready when value == StateType.InShop:
                // Attacking => InShop
                case StateType.Attacking when value == StateType.InShop:
                    field = value;
                    break;
                default:
                    return;
            }

            switch (field)
            {
                // 状態遷移時の処理
                case StateType.Reloading:
                {
                    AimEntity.Mode = _targetMode;
                    AimEntity.MinRange = _minRange;
                    AimEntity.MaxRange = _maxRange;
                    AimEntity.RotateSensitivity = _rotateSensitivity;

                    // すべて打ち切ったらクールダウン (リロード) を開始
                    RestartCoolDown();
                    break;
                }
                case StateType.Ready:
                {
                    _enterTargetWaiter?.Start(5f, () => { StateMachine = StateType.Attacking; });
                    break;
                }
                case StateType.Attacking:
                {
                    _ = DoAttackLocal();
                    break;

                    async ValueTask DoAttackLocal()
                    {
                        AimEntity.RotateSensitivity = _rotateSensitivityAttacking;
                        // ToDo: 射撃中に遷移したときのキャンセル処理が必要
                        await DoAttack();
                        StateMachine = StateType.Reloading;
                    }
                }
                case StateType.InShop:
                {
                    _enterTargetWaiter?.Cancel();
                    break;
                }
            }
        }
    } = StateType.InShop;

    public override void _ExitTree()
    {
        _enterTargetWaiter?.Dispose();
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        StateMachine = StateType.Ready;
    }

    private protected override void OnStartAttack(uint level)
    {
        StateMachine = StateType.Reloading;
    }

    private protected override void OnStopAttack()
    {
        StateMachine = StateType.InShop;
    }

    private async ValueTask DoAttack()
    {
        // マガジンの弾を FireRate 間隔で撃ち切る
        for (var i = 0; i < _magazineSize; i++)
        {
            // 2発目以降は FireRate 間隔 を待機する
            if (i >= 1)
            {
                var frameCounter = 0u;
                while (frameCounter < _fireRate)
                {
                    frameCounter++;
                    await this.NextPhysicsFrameAsync();
                }
            }

            var prj = _projectile.Instantiate<BulletProjectile>();
            {
                prj.Damage = State.Damage.CurrentValue;
                prj.Knockback = State.Knockback.CurrentValue;
                prj.LifeFrame = _life;
                prj.PenetrateSettings = (BulletProjectile.PenetrateType)_penetrate;
            }

            if (_muzzle is not null)
            {
                prj.ConstantForce = _muzzle.GlobalTransform.X * _speed;
                AddProjectile(prj, _muzzle.GlobalPosition);
            }
            else
            {
                prj.ConstantForce = AimEntity.GlobalTransform.X * _speed;
                AddProjectile(prj, GlobalPosition);
            }

            // Note: AddTo はシーン内にないとエラーになるので, AddProjectile の後に呼ぶ
            // ToDo: 暫定実装, 敵にヒットするたびにダメージを半分にする
            if (prj.PenetrateSettings.HasFlag(BulletProjectile.PenetrateType.Enemy))
            {
                prj.Hit
                    .Where(x => x.HitNode is EntityEnemy)
                    .Subscribe(prj, (x, s) => { s.Damage *= 0.5f; })
                    .AddTo(prj);
            }

            // ToDo: 仮
            // Sprite の Animation
            {
                var sprite = GetNode<Sprite2D>("%Sprite");
                var tween = CreateTween();
                var startPos = sprite.Position;
                var halfTime = _fireRate / (60f * 2f);
                tween.TweenProperty(sprite, "position", startPos - new Vector2(-5f, 0), halfTime);
                tween.TweenProperty(sprite, "position", startPos, halfTime);
            }
        }
    }

    private enum StateType
    {
        InShop,
        Reloading,
        Ready,
        Attacking
    }
}