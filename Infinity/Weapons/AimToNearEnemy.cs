using Godot;

namespace fms;

public partial class AimToNearEnemy : Area2D
{
    public bool IsAiming { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        var overlap = GetOverlappingBodies();
        var len = 99999f;
        Enemy? nearestEnemy = null;

        foreach (var o in overlap)
        {
            if (o is Enemy enemy)
            {
                var d = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
                if (d < len)
                {
                    len = d;
                    nearestEnemy = enemy;
                }
            }
        }

        if (nearestEnemy != null)
        {
            // Update ROtation
            var angle = Mathf.Atan2(nearestEnemy.GlobalPosition.Y - GlobalPosition.Y,
                nearestEnemy.GlobalPosition.X - GlobalPosition.X);
            Rotation = angle;
            IsAiming = true;
        }
        else
        {
            // Update Rotation
            var angle = Mathf.Atan2(0, 1);
            Rotation = angle;
            IsAiming = false;
        }
    }
}