using Godot;

namespace fms;

public interface IGodotNode
{
    void AddChild(Node child);

    void RemoveChild(Node child);
}

public interface IEntity : IGodotNode
{
    /// <summary>
    /// Indicates whether the entity is dead.
    /// </summary>
    bool IsDead { get; }

    /// <summary>
    /// Gets the current golobal position of this entity
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// </summary>
    EntityState State { get; }

    /// <summary>
    /// ダメージを与える
    /// </summary>
    void ApplayDamage(float amount, IEntity instigator, Node causer, string causerPath);
}