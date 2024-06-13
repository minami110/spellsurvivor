using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace fms.Projectile;

[Flags]
public enum WhyDead
{
    Life,
    Short,
    Collision
}

public class SpellBlocker
{
    public VectorGenerator Generator = null!;
    public PackedScene Scene;

    public SpellBlocker At(VectorGenerator generator)
    {
        Generator = generator;
        return this;
    }

    public SpellBlocker Next(PackedScene scene, WhyDead when)
    {
        throw new NotImplementedException();
    }

    internal void AddSpell(PackedScene scene)
    {
        Scene = scene;
    }
}

public class SpellBlock
{
    private SpellBlocker? _next;
    private readonly Vector2 _position;
    private readonly Vector2 _velocity;

    public SpellBlock(Vector2 pos, Vector2 dir)
    {
        _position = pos;
        _velocity = dir;
    }

    public SpellBlocker Spawn(PackedScene scene)
    {
        _next = new SpellBlocker();
        _next.AddSpell(scene);
        return _next;
    }

    public async ValueTask StartAsync(Node parent, CancellationToken token = default)
    {
        if (_next is not null)
        {
            _next.Generator.SetPrevPositionAndVelocity(_position, _velocity);
            if (_next.Generator.GetPositionAndVelocity(out var pos, out var vel))
            {
                var projectile = _next.Scene.Instantiate<BaseProjectile>();
                projectile.GlobalPosition = pos;
                projectile.Direction = vel;
                parent.AddChild(projectile);
                _next = null;
            }
        }
    }
}

public abstract class VectorGenerator
{
    protected Vector2 PrevPosition;
    protected Vector2 PrevVelocity;

    public abstract bool GetPositionAndVelocity(out Vector2 position, out Vector2 velocity);

    public void SetPrevPositionAndVelocity(Vector2 position, Vector2 velocity)
    {
        PrevPosition = position;
        PrevVelocity = velocity;
    }
}

public class Inherit : VectorGenerator
{
    public override bool GetPositionAndVelocity(out Vector2 position, out Vector2 velocity)
    {
        position = PrevPosition;
        velocity = PrevVelocity;
        return true;
    }
}

public class Reflect : VectorGenerator
{
    public override bool GetPositionAndVelocity(out Vector2 position, out Vector2 velocity)
    {
        throw new NotImplementedException();
    }
}

public class SearchNearestEnemy(float minRadius, float maxRadius) : VectorGenerator
{
    public override bool GetPositionAndVelocity(out Vector2 position, out Vector2 velocity)
    {
        throw new NotImplementedException();
        /*
        position = Vector2.Zero;
        velocity = Vector2.Zero;

        Enemy? nearestEnemy = null;
        var nearestDistance = 99999f;

        var overlappingArea = _trickshotSearchArea.GetOverlappingBodies();
        if (overlappingArea.Count <= 0)
        {
            return false;
        }

        foreach (var body in overlappingArea)
        {
            // Enemy ではないか, 無視対象のばあいはスキップ
            if (body is not Enemy enemy)
            {
                continue;
            }

            // 最も近い敵を探す
            var d = PrevPosition.DistanceTo(enemy.GlobalPosition);
            if (d >= nearestDistance)
            {
                continue;
            }

            nearestDistance = d;
            nearestEnemy = enemy;
        }

        return nearestEnemy is not null;
        */
    }
}