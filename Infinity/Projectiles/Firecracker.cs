using fms.Weapon;
using Godot;
using R3;

namespace fms.Projectile;

public partial class Firecracker : ProjectileRigidBodyBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;
    

    
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;
    
    public Node BulletSpawnNode = null!;
    
    public FirecrackerSparkData FirecrackerSparkDataSettings;
    
    //trickshot settings
    [Export]
    private Area2D _trickshotSearchArea = null!;
   
    [Export]
    private CollisionShape2D _trickshotSearchCollision = null!;
    
    // trickshot bounce
    public FirecrackerTrickshotArrowData FirecrackerTrickshotArrowData;
    
    public override void _Ready()
    {
        // BounceCount が 0 以上の場合は 敵を検索する Collision を有効にする
        if (FirecrackerTrickshotArrowData.BounceCount > 0)
        {
            _trickshotSearchCollision.Disabled = false;
            _trickshotSearchCollision.GlobalScale = new Vector2(FirecrackerTrickshotArrowData.BounceSearchRadius, FirecrackerTrickshotArrowData.BounceSearchRadius);
        }
        else
        {
            _trickshotSearchCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }
        
        // Connect
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);
    }

    private Enemy? _prevTargetEnemy;
    private int _trickshotBounceCounter;
    private void OnEnemyBodyEntered(Enemy enemy)
    {
        // trickshot 関係の処理
        if (_prevTargetEnemy == enemy)
        {
            return;
        }
        
        // TODO 後でダメージの係数計算をする
        
        // ダメージ床を生成する
        SpawnFirecrackerSparks();
        
        /*
        _prevTargetEnemy = enemy;
        
        // 残りの Bounce 回数が残っていなければ Projectile を殺す
        if (_trickshotBounceCounter >= FirecrackerTrickshotArrowData.BounceCount)
        {
            GD.PushError("Bounce Count is over");
            //KillThis();
            return;
        }

        // 次の Bounce の処理を行う
        ResetAge();
        _trickshotBounceCounter++;

        // Bounce 範囲の敵を検索する, 見つかった場合は弾の方向を更新する
        if (TryGetNearestEnemy(_prevTargetEnemy, out var nextEnemy))
        {
            // Update Direction
            LinearVelocity =
                (nextEnemy!.GlobalPosition - GlobalPosition).Normalized() * InitialSpeed;
            Rotation = LinearVelocity.Angle();
        }
        else
        {
            GD.PushError("Next Enemy is not found");
            //KillThis();
        }
        */
    }
    
    private void SpawnFirecrackerSparks()
    {
        var bullet = _bulletPackedScene.Instantiate<FirecrackerSparks>();
        {
            bullet.GlobalPosition = GlobalPosition;
            bullet.FirecrackerSparkDataSettings = FirecrackerSparkDataSettings;
        }
        BulletSpawnNode.AddChild(bullet);
    }
    
    /// <summary>
    ///     近くの敵を検索する
    /// </summary>
    /// <param name="ignoreEnemy">無視する Enemy</param>
    /// <param name="nearestEnemy">最も近い Enemy が代入される</param>
    /// <returns>Enemy が見つかった場合は true</returns>
    private bool TryGetNearestEnemy(Enemy? ignoreEnemy, out Enemy? nearestEnemy)
    {
        nearestEnemy = null;
        var nearestDistance = 99999f;

        var overlappingArea = _trickshotSearchArea.GetOverlappingBodies();
        if (overlappingArea.Count <= 0)
        {
            return false;
        }

        foreach (var body in overlappingArea)
        {
            // Enemy ではないか, 無視対象のばあいはスキップ
            if (body is not Enemy enemy || enemy == ignoreEnemy)
            {
                continue;
            }

            // 最も近い敵を探す
            var d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (d >= nearestDistance)
            {
                continue;
            }

            nearestDistance = d;
            nearestEnemy = enemy;
        }

        return nearestEnemy is not null;
    }
}

/// <summary>
///     爆竹の火花の初期化用データ
/// </summary>
public readonly struct FirecrackerSparkData
{
    public required int DamageCoolDownFrame { get; init; }
    public required float BaseDamage { get; init; }
    public required float DamageAreaRadius { get; init; }
    public required float LifeFrame { get; init; }
}

public readonly struct FirecrackerTrickshotArrowData
{
    public required int BounceCount { get; init; }
    public required float BounceDamageMultiplier { get; init; }
    public required float BounceSearchRadius { get; init; }
}
