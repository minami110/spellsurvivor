using Godot;

namespace fms.Projectile;

/// <summary>
/// Projectile を対象の周りを回るような挙動に上書きする Mod
/// Note: Speed は Proj のものを使用する
/// </summary>
public partial class Orbit : Node
{
    private BaseProjectile _projectile = null!;

    /// <summary>
    /// Orbit の中心となる対象
    /// </summary>
    public required Node2D Target { get; init; }

    /// <summary>
    /// 半径 (px)
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    /// オフセット角度 (Degree)
    /// </summary>
    public float OffsetDeg { get; init; } = 0f;

    /// <summary>
    /// 周回速度 (Degree/s)
    /// </summary>
    public float Speed { get; init; } = 0f;

    public override void _Ready()
    {
        var prj = GetParentOrNull<BaseProjectile>();
        if (prj is null)
        {
            SetPhysicsProcess(false);
            return;
        }

        _projectile = prj;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Target.IsInsideTree() || !IsInstanceValid(Target))
        {
            return;
        }

        // Note: 滑らかな回転にするため UnixTime をベースにする
        var currentTime = Time.GetUnixTimeFromSystem() * Mathf.DegToRad(Speed) + Mathf.DegToRad(OffsetDeg);
        var positionX = (float)Mathf.Cos(currentTime);
        var positionY = (float)Mathf.Sin(currentTime);
        var unit = new Vector2(positionX, positionY);

        var targetPos = Target.GlobalPosition + unit * Radius;
        var deltaPos = targetPos - _projectile.GlobalPosition;
        _projectile.AddLinearVelocity(deltaPos / (float)delta);
    }
}