using System;
using Godot;

namespace fms.Enemy;

/// <summary>
/// EnemyBase の子に配置して移動に応じて Sprite の方向を制御するノード
/// </summary>
public partial class EnemyAnimator : Node
{
    /// <summary>
    /// ぷにぷにする速度の Scale
    /// </summary>
    [Export(PropertyHint.Range, "0.0,5.0")]
    private float _puniSpeed = 1.0f;

    /// <summary>
    /// ぷにぷにする深さの Scaling, 元 Sprite の相対な大きさになる
    /// </summary>
    [Export(PropertyHint.Range, "0.0,2.0")]
    private float _puniDepth = 0.06f;

    /// <summary>
    /// 元 Sprite が左を向いている Texture の場合はチェックを入れる
    /// </summary>
    [Export]
    private bool _invertFlipDirection;

    private Vector2 _defaultScale = Vector2.One;
    private EnemyBase _enemy = null!;

    private float _puniLocation;
    private Sprite2D _sprite = null!;

    public override void _Ready()
    {
        if (_puniDepth <= 0 || _puniSpeed <= 0)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            return;
        }

        var enemy = GetParentOrNull<EnemyBase>();
        _enemy = enemy ?? throw new InvalidProgramException("親に EnemyBase が見つかりませんでした");

        var sprite = GetNodeOrNull<Sprite2D>("../Sprite");

        _sprite = sprite ?? throw new InvalidProgramException("兄弟に Sprite が見つかりませんでした");
        _defaultScale = _sprite.Scale;
    }

    public override void _Process(double delta)
    {
        // Note: 死亡時アニメーションがあるのでこっちでの再生はやめる
        // ToDo: Animator なんだから こっちで巻き取ったほうがいいかも
        if (_enemy.IsDead)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            return;
        }

        // 現在の移動方向を取得する
        var vel = _enemy.LinearVelocity; // 実際の移動ベクトル (ノックバック中は後ろ)
        var dir = _enemy.TargetVelocity; // 本人が思っている方向 (ノックバック中でもプレイヤー)

        // 右に移動している場合は右に, 左に移動している場合は左を向くように FlipH を制御する
        if (dir.X > 0)
        {
            _sprite.FlipH = _invertFlipDirection;
        }
        else if (dir.X < 0)
        {
            _sprite.FlipH = !_invertFlipDirection;
        }

        // 移動に合わせて Sprite を上下にぷにぷに, 方向転換させる実装
        // 物理演算による速度と, スポーン時に設定されたデータの速度のうち小さい方を取得する
        // Note: ぶっ飛んでいるとき早くなりすぎず, 停止しているときはぷにぷにしないようにするため
        var physicsSpeed = vel.Length();
        var dataSpeed = _enemy.State.MoveSpeed.CurrentValue;
        var speed = MathF.Min(physicsSpeed, dataSpeed);

        // ぷにぷにの位置を更新する
        _puniLocation += speed * _puniSpeed * (float)delta * 0.1f;

        // Sprite を変形させる
        var puniScale = MathF.Sin(_puniLocation) * _puniDepth;
        var scaleX = 1f - puniScale * 0.6f;
        var scaleY = 1f + puniScale;
        _sprite.Scale = _defaultScale * new Vector2(scaleX, scaleY);
    }
}