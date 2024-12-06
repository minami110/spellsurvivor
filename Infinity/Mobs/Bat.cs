using System;
using Godot;
using R3;

namespace fms.Mob;

public partial class Bat : RigidBody2D, IEntity
{
    private uint _age;
    private float _changeDirectionRate;

    private float _changeDirectionSpeed;
    private Vector2 _currentDirection;
    public float Damage { get; set; } = 10f;

    public uint Lifetime { get; set; } = 240u;

    public float MoveSpeed { get; set; } = 100f;

    public override void _EnterTree()
    {
        _changeDirectionSpeed = (float)GD.Randfn(0.5f, 0.2f);
        _currentDirection = GlobalTransform.X;

        // Damage area subscription
        var damageArea = GetNode<Area2D>("%Damage");
        damageArea.BodyEnteredAsObservable()
            .Subscribe(node =>
            {
                if (node is EntityEnemy enemy)
                {
                    ((IEntity)enemy).ApplayDamage(Damage, this, this);
                }
            })
            .AddTo(this);

        // Play sprite animation
        GetNode<AnimatedSprite2D>("%Sprite").Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_age++ >= Lifetime)
        {
            QueueFree();
            return;
        }

        _changeDirectionRate += (float)delta * _changeDirectionSpeed;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // 方向転換の確率を抽選する
        if (GD.Randf() >= _changeDirectionRate)
        {
            return;
        }

        // 方向転換を行う
        _changeDirectionRate = 0f;

        // ランダムな方向を抽選する
        // 0 を中心として, ~180 - 180 の範囲でランダムな角度を取得する
        var rotateAngle = (float)GD.Randfn(0f, 2 * Mathf.Pi);

        // targetAngle の角度だけ currentDirection を回転させる
        _currentDirection = _currentDirection.Rotated(rotateAngle);

        // 速度を設定する
        LinearVelocity = _currentDirection * MoveSpeed;
    }

    void IGodotNode.AddChild(Node child)
    {
        AddChild(child);
    }

    bool IEntity.IsDead => throw new NotImplementedException();

    EntityState IEntity.State => throw new NotImplementedException();

    void IEntity.ApplayDamage(float amount, IEntity instigator, Node causer)
    {
        // Do nothing
    }
}