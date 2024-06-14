using Godot;

namespace fms;

public partial class AimToNearEnemy : Area2D
{
    private float _lastPlayerAngle;
    public bool IsAiming { get; private set; }
    public Enemy? NearestEnemy { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        var overlap = GetOverlappingBodies();
        var len = 999999f;
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
            IsAiming = true;
            NearestEnemy = nearestEnemy;
            Rotation = angle;
        }
        else
        {
            // Update Rotation
            var parent = GetParent().GetParent<MeMe>();
            if (parent.MoveDirection.X > 0)
            {
                var angle = Mathf.Atan2(0, 1);
                _lastPlayerAngle = angle;
                Rotation = angle;
            }
            else if (parent.MoveDirection.X < 0)
            {
                var angle = Mathf.Atan2(0, -1);
                _lastPlayerAngle = angle;
                Rotation = angle;
            }

            IsAiming = false;
            NearestEnemy = null;
        }
    }
}