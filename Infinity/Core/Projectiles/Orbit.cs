using Godot;

namespace fms.Projectile;

/// <summary>
/// Projectile を対象の周りを回るような挙動に上書きする Mod
/// Note: Speed は Proj のものを使用する
/// </summary>
public partial class Orbit : Node
{
    public required Node2D Target { get; init; }

    public float Radius { get; init; }

    public float OffsetDeg { get; init; } = 0f;

    public override void _PhysicsProcess(double delta)
    {
        if (!Target.IsInsideTree() || !IsInstanceValid(Target))
        {
            return;
        }

        var projectile = GetParent<BaseProjectile>();
        var speedRad = Mathf.DegToRad(projectile.Speed);

        // Note: 滑らかな回転にするため UnixTime をベースにする
        var currentTime = Time.GetUnixTimeFromSystem() * speedRad + Mathf.DegToRad(OffsetDeg);
        var positionX = (float)Mathf.Cos(currentTime);
        var positionY = (float)Mathf.Sin(currentTime);
        var unit = new Vector2(positionX, positionY);

        projectile.GlobalPosition = Target.GlobalPosition + unit * Radius;
    }
}