using Godot;
using R3;

namespace spellsurvivor;

public partial class MeMe : CharacterBody2D, IPawn, IEntity
{
    private readonly Subject<DeadReason> _deadSubject = new();

    private Vector2 _nextMoveDirection;

    [Export(PropertyHint.Range, "0,1000,50")]
    public float MoveSpeed { get; private set; } = 200;

    [Export(PropertyHint.Range, "0,100,")]
    public float Health { get; private set; } = 100f;

    [Export(PropertyHint.Range, "0,100,")]
    public float MaxHealth { get; private set; } = 100f;

    /// <summary>
    /// </summary>
    public Observable<DeadReason> Dead => _deadSubject;

    public Race Race => Race.Player;

    void IEntity.TakeDamage(float amount, IEntity? instigator)
    {
        if (instigator is null)
        {
            return;
        }

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            // Emit signal to main scene
            _deadSubject.OnNext(new DeadReason(instigator.Race.ToString(), "Attack"));
        }
    }

    void IPawn.PrimaryPressed()
    {
    }

    void IPawn.PrimaryReleased()
    {
    }

    void IPawn.MoveForward(in Vector2 dir)
    {
        _nextMoveDirection = dir;
    }

    public override void _ExitTree()
    {
        _deadSubject.Dispose();
    }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (_nextMoveDirection.LengthSquared() > 0f)
        {
            // Update PlayerForward
            var angle = Mathf.Atan2(_nextMoveDirection.Y, _nextMoveDirection.X);
            Rotation = angle;

            // Update Position
            var motion = _nextMoveDirection * (float)delta * MoveSpeed;
            MoveAndCollide(motion);
        }
    }
}