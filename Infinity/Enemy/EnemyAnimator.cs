using System;
using Godot;

namespace fms.Enemy;

/// <summary>
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

    [Export]
    private bool _invertFlipDirection = false;

    private Vector2 _defaultScale = Vector2.One;
    private EnemyBase _enemy = null!;
    private Sprite2D _sprite = null!;

    private float _puniLocation = 0.0f;

    public override void _Ready()
    {
        if (_puniDepth <= 0 || _puniSpeed <= 0)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            return;
        }

        var enemy = GetParentOrNull<EnemyBase>();
        if (enemy is null)
        {
            throw new InvalidProgramException("親に EnemyBase が見つかりませんでした");
        }

        _enemy = enemy;

        var sprite = GetNodeOrNull<Sprite2D>("../Sprite");

        if (sprite is null)
        {
            throw new InvalidProgramException("兄弟に Sprite が見つかりませんでした");
        }

        _sprite = sprite;
        _defaultScale = _sprite.Scale;
    }

    public override void _Process(double delta)
    {
        // Note: いろいろ仮です

        // 現在の移動方向を取得する
        var vel = _enemy.LinearVelocity;
        var dir =  vel.Normalized();

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
        var scaleY = 1f + MathF.Sin(_puniLocation) * _puniDepth;
        _sprite.Scale = _defaultScale * new Vector2(1f, scaleY);
    }
}