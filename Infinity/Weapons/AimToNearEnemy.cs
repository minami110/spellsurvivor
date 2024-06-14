using Godot;

namespace fms;

public partial class AimToNearEnemy : Area2D
{
    public override void _PhysicsProcess(double delta)
    {
        var overlap = GetOverlappingBodies();
        var len = 99999f;
        Enemy? _nearestEnemy = null;

        foreach (var o in overlap)
        {
            if (o is Enemy enemy)
            {
                var d = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
                if (d < len)
                {
                    len = d;
                    _nearestEnemy = enemy;
                }
            }
        }

        if (_nearestEnemy != null)
        {
            // Update ROtation
            var angle = Mathf.Atan2(_nearestEnemy.GlobalPosition.Y - GlobalPosition.Y,
                _nearestEnemy.GlobalPosition.X - GlobalPosition.X);
            Rotation = angle;
        }
        else
        {
            // Update ROtation
            var angle = Mathf.Atan2(0, 1);
            Rotation = angle;
        }
    }
}