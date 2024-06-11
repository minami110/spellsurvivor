using Godot;
using R3;

namespace fms.Projectile;

public partial class RotateBook : ProjectileNode2DBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    /// <summary>
    ///     中心距離からの半径距離 (px)
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    ///     一周するのにかかる秒数
    /// </summary>
    public float SpeedDegreePerSecond { get; set; }

    /// <summary>
    ///     円周上における角度のオフセット (複数弾を描写するときにずらすように)
    /// </summary>
    public float OffsetAngleDegree { get; set; }

    public override void _Ready()
    {
        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
    }

    public override void _Process(double delta)
    {
        // 毎フレーム自分の位置を計算する
        GlobalPosition = CalculatePosition(Mathf.DegToRad(SpeedDegreePerSecond), Radius);
    }

    private Vector2 CalculatePosition(float speedRadiun, float radius)
    {
        // Note: 滑らかな回転にするため UnixTime をベースにする
        var currentTime = Time.GetUnixTimeFromSystem() * speedRadiun + Mathf.DegToRad(OffsetAngleDegree);
        var positionX = (float)Mathf.Cos(currentTime);
        var positionY = (float)Mathf.Sin(currentTime);
        var unitPosition = new Vector2(positionX, positionY);

        var playerNode = this.GetPlayerNode();
        if (playerNode is Node2D player)
        {
            return player.GlobalPosition + unitPosition * radius;
        }

        return unitPosition * radius;
    }

    private void OnEnemyBodyEntered(Enemy enemy)
    {
        enemy.TakeDamage(BaseDamage);
    }
}