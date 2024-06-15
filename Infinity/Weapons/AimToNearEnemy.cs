using Godot;
using R3;

namespace fms;

public partial class AimToNearEnemy : Area2D
{
    [Export]
    private bool UpdateRotation { get; set; } = true;

    [Export(PropertyHint.Range, "0,1")]
    private float RotateSensitivity { get; set; } = 0.7f;

    private float _restAngle;

    private float _targetAngle;

    public bool IsAiming { get; private set; }
    public Enemy? NearestEnemy { get; private set; }

    public override void _EnterTree()
    {
        // Subscribe to parent player's face direction
        var parent = GetParent().GetParent<MeMe>(); // ToDo: Hardcoded
        parent.FaceDirection
            .Subscribe(x => { _restAngle = x == PawnFaceDirection.Right ? Mathf.Atan2(0, 1) : Mathf.Atan2(0, -1); })
            .AddTo(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        var overlap = GetOverlappingBodies();
        var len = 9999999f;
        Enemy? nearestEnemy = null;

        foreach (var o in overlap)
        {
            if (o is not Enemy enemy)
            {
                continue;
            }

            var d = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (d >= len)
            {
                continue;
            }

            len = d;
            nearestEnemy = enemy;
        }

        if (nearestEnemy != null)
        {
            IsAiming = true;
            NearestEnemy = nearestEnemy;

            if (!UpdateRotation)
            {
                return;
            }

            var targetAngle = Mathf.Atan2(nearestEnemy.GlobalPosition.Y - GlobalPosition.Y,
                nearestEnemy.GlobalPosition.X - GlobalPosition.X);
            Rotation = Mathf.LerpAngle(Rotation, targetAngle, RotateSensitivity);
        }
        else
        {
            IsAiming = false;
            NearestEnemy = null;

            if (!UpdateRotation)
            {
                return;
            }

            // Update Rotation
            Rotation = Mathf.LerpAngle(Rotation, _restAngle, RotateSensitivity);
        }
    }
}