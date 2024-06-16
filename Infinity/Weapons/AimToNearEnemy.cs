using Godot;
using Godot.Collections;
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
        if (FindNearestEnemy(GetOverlappingBodies(), out var nearestEnemy))
        {
            IsAiming = true;
            NearestEnemy = nearestEnemy;

            if (!UpdateRotation)
            {
                return;
            }

            var targetAngle = Mathf.Atan2(nearestEnemy!.GlobalPosition.Y - GlobalPosition.Y,
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

    private bool FindNearestEnemy(Array<Node2D> bodies, out Enemy? enemy)
    {
        var len = float.MaxValue;
        enemy = null;

        foreach (var o in bodies)
        {
            if (o is not Enemy e)
            {
                continue;
            }

            var distance = GlobalPosition.DistanceSquaredTo(e.GlobalPosition);
            if (distance < len)
            {
                len = distance;
                enemy = e;
            }
        }

        return enemy is not null;
    }
}