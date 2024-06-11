using Godot;
using R3;

namespace fms.Projectile;

/// <summary>
///     Trickshot Faction 挙動が実装された Arrow
/// </summary>
public partial class TrickshotArrow : ProjectileRigidBodyBase
{
    [Export]
    private Area2D _enemyDamageArea = null!;

    [Export]
    private Area2D _trickshotSearchArea = null!;

    [Export]
    private CollisionShape2D _trickshotSearchCollision = null!;

    [Export]
    private TextureRect _sprite = null!;

    [ExportCategory("Sounds")]
    [Export]
    private AudioStream _spawnSound = null!;

    [Export]
    private AudioStream _hitSound = null!;

    private Enemy? _prevTargetEnemy;

    private int _trickshotBounceCounter;

    public int BounceCount { get; set; }
    public float BounceDamageMultiplier { get; set; }
    public float BounceSearchRadius { get; set; }


    public override void _Ready()
    {
        // Bounce Count が 0 以上の場合は 敵を検索する Collision を有効にする
        // 0 の場合は必要ないので無効化する
        if (BounceCount > 0)
        {
            // Set collision shape
            _trickshotSearchCollision.Disabled = false;
            _trickshotSearchCollision.GlobalScale = new Vector2(BounceSearchRadius, BounceSearchRadius);
        }
        else
        {
            _trickshotSearchCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        }

        // ダメージ判定の Area を購読する
        _enemyDamageArea.BodyEnteredAsObservable()
            .Cast<Node2D, Enemy>()
            .Subscribe(this, (x, state) => { state.OnEnemyBodyEntered(x); })
            .AddTo(this);

        // Spawn 時の Sound を再生する
        SoundManager.PlaySoundEffect(_spawnSound);
    }

    private void OnEnemyBodyEntered(Enemy enemy)
    {
        if (_prevTargetEnemy == enemy)
        {
            return;
        }

        // 敵に与えるダメージを決定する, Bounce 時 はスケールを加味する
        var damage = BaseDamage;
        if (_trickshotBounceCounter > 0)
        {
            damage *= BounceDamageMultiplier;
        }

        // 敵にダメージを与える
        enemy.TakeDamage(damage);
        _prevTargetEnemy = enemy;

        // ヒット時のサウンドを再生する
        SoundManager.PlaySoundEffect(_hitSound);

        // 残りの Bounce 回数が残っていなければ Projectile を殺す
        if (_trickshotBounceCounter >= BounceCount)
        {
            KillThis();
            return;
        }

        // 次の Bounce の処理を行う
        ResetAge();
        _sprite.Modulate = new Color(0, 1, 0);
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
            KillThis();
        }
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