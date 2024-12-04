using System;
using fms.Projectile;
using Godot;

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

    [Export]
    private Node2D? _muzzle;

    // ==== Aim Settings ====
    [ExportGroup("Aim Settings")]
    [Export(PropertyHint.Range, "0,100,,or_greater,suffix:px")]
    private float _minRange;

    [Export(PropertyHint.Range, "0,200,,or_greater,suffix:px")]
    private float _maxRange = 100f;

    /// <summary>
    /// 敵を狙う速度の感度 (0 ~ 1), 1 で最速, 0 で全然狙えない
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    private float _rotateSensitivity = 0.3f;

    private AimEntity? _aimEntity;
    private AimEntityEnterTargetWaiter? _aimEntityEnterTargetWaiter;

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
            _aimEntityEnterTargetWaiter = new AimEntityEnterTargetWaiter(_aimEntity);
            return _aimEntity;
        }
    }

    private Vector2 ProjectileSpawnPosition => _muzzle?.GlobalPosition ?? GlobalPosition;

    public override void _Ready()
    {
        AimEntity.Mode = AimEntity.TargetMode.NearestEntity;
        AimEntity.MinRange = _minRange;
        AimEntity.MaxRange = _maxRange;
        AimEntity.RotateSensitivity = _rotateSensitivity;
    }

    public override void _ExitTree()
    {
        _aimEntityEnterTargetWaiter?.Dispose();
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        _aimEntityEnterTargetWaiter?.Start(5f, SpawnProjectile);
    }

    private protected override void OnStopAttack()
    {
        _aimEntityEnterTargetWaiter?.Cancel();
    }

    private void SpawnProjectile()
    {
        var prj = _projectile.Instantiate<BulletProjectile>();
        {
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.LifeFrame = _life;
            prj.ConstantForce = AimEntity.GlobalTransform.X * _speed;
            prj.PenetrateSettings = (BulletProjectile.PenetrateType)_penetrate;
        }

        AddProjectile(prj, ProjectileSpawnPosition);
        RestartCoolDown();
    }
}