using System;

namespace fms.Projectile;

[Flags]
public enum WhyDead : ulong
{
    Life = 1 << 0,
    Short = 1 << 1,
    CollidedWithWall = 1 << 2,
    CollidedWithEnemy = 1 << 3,

    CollidedWithAny = CollidedWithWall | CollidedWithEnemy
}