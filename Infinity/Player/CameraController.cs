using Godot;

namespace fms;

public partial class CameraController : Area2D
{
    [Export]
    private float _moveSpeed = 2f;

    [Export]
    private float _zoomSpeed = 0.4f;

    private Camera2D _camera = null!;
    private Vector2 _targetPosition;
    private float _targetZoom;

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("../MainCamera");
    }

    public override void _Process(double delta)
    {
        // 敵の平均位置を求める
        // その際に、自分の位置から近い敵の位置を重視するような計算を行う
        var bodies = GetOverlappingBodies();
        var currentPos = GlobalPosition;
        var avgDir = Vector2.Zero;
        var mostFarDistance = 0f;

        var count = 0;
        foreach (var body in bodies)
        {
            if (body is EnemyBase)
            {
                var enemyPos = body.GlobalPosition;
                var vector = enemyPos - currentPos;
                var distance = vector.Length();
                if (distance > mostFarDistance)
                {
                    mostFarDistance = distance;
                }

                var weight = Mathf.Max(25f / distance, 1f);
                avgDir += vector * weight;
                count++;
            }
        }

        if (mostFarDistance == 0f)
        {
            _targetZoom = 2.0f;
        }
        else
        {
            _targetZoom = Mathf.Clamp(2f - Mathf.Pow(mostFarDistance / 300f, 2), 1.6f, 2.0f);
        }


        if (count > 0)
        {
            avgDir /= count;
            _targetPosition = GlobalPosition + avgDir;
        }
        else
        {
            _targetPosition = GlobalPosition;
        }

        var newZoom = new Vector2(_targetZoom, _targetZoom);
        _camera.GlobalPosition = _camera.GlobalPosition.Lerp(_targetPosition, _moveSpeed * (float)delta);
        _camera.Zoom = _camera.Zoom.Lerp(newZoom, _zoomSpeed * (float)delta);
    }
}