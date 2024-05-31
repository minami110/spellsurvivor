using System.Collections.Generic;
using fms.Effect;
using fms.Faction;
using fms.Projectile;
using Godot;

namespace fms.Minion;

public partial class ForwardKnife : MinionBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    // この Minion が所属する Faction の一覧
    private static readonly FactionBase[] _factions =
    {
        new Bruiser(),
        new Duelist(),
        new Knight(),
        new Trickshot()
    };

    private int _trickshotBounceCount;

    private protected override int BaseCoolDownFrame => 60;

    public override FactionBase[] Factions => _factions;

    private protected override void DoAttack()
    {
        switch (Level.CurrentValue)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet(GlobalPosition);
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(GlobalPosition, 10f);
                SpawnBullet(GlobalPosition, -10f);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(GlobalPosition, 20f);
                SpawnBullet(GlobalPosition, 0f, 10f);
                SpawnBullet(GlobalPosition, -20f);
                break;
            }
        }
    }


    private protected override void OnSolveEffect(IReadOnlyList<EffectBase> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect)
            {
                case TrickshotBounceCount trickshotBounceCount:
                {
                    _trickshotBounceCount += (int)trickshotBounceCount.Value;
                    break;
                }
            }
        }
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        var bullet = _bulletPackedScene.Instantiate<TrickshotArrow>();
        {
            bullet.BaseSpeed = 1000f;
            bullet.BaseDamage = MinionCoreData.BaseAttack;
            bullet.InitialVelocity = GlobalTransform.X; // Forward
            bullet.InitialPosition = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
            bullet.BounceCount = _trickshotBounceCount + 2;
            bullet.BounceDamageMultiplier = 0.4f; // ToDo:
            bullet.SearchRadius = 400f;
        }

        _bulletSpawnNode.AddChild(bullet);
    }
}