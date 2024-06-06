using Godot;
using R3;

namespace fms.Projectile;

public partial class RotateBook : ProjectileBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    [Export]
    private RigidBody2D _rigidBody = null!;

    private float _angularVelocity;

    private double _timer;

    public float Radius { get; set; }

    public float SecondPerRound { get; set; } = 1f;

    public float Angle { get; set; }

    /// <summary>
    ///     複数の弾を同時に発射するときの初期位置のずれを設定するために利用する時間(second)
    ///     (クールダウン / 弾数)で導出する
    /// </summary>

    // (中桐)名前終わっています 単位も統一できていないです
    public float InitTimeForRelativePos { get; set; }

    public override void _Ready()
    {
        _rigidBody.Hide();

        // Set rigidbody parameter
        _rigidBody.GlobalPosition = CalculatePosition(_timer, _angularVelocity, Radius);
        _rigidBody.RotationDegrees = Angle;

        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);

        // Calculate Angular Velocity
        _angularVelocity = Mathf.DegToRad(360 / SecondPerRound);

        // Set relative position
        _timer = InitTimeForRelativePos;

        _rigidBody.Show();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _timer += delta;

        _rigidBody.GlobalPosition = CalculatePosition(_timer, _angularVelocity, Radius);
    }

    private Vector2 CalculatePosition(double time, float angularVelocity, float radius)
    {
        var positionX = (float)Mathf.Cos(time * angularVelocity);
        var positionY = (float)Mathf.Sin(time * angularVelocity);

        var unitVec = new Vector2(positionX, positionY);

        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode is Node2D player)
        {
            return player.GlobalPosition + unitVec * radius;
        }

        return unitVec * radius;
    }

    private void OnEnemyBodyEntered(Enemy enemy)
    {
        enemy.TakeDamage(BaseDamage);
    }
}