using Godot;

namespace fms;

public interface IEntity
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
    /// ダメージを与える
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="instigator">ダメージを引き起こした Entity (例: プレイヤー)</param>
    /// <param name="causer">実際に被害を引き起こした主体 例: 手榴弾 Prj ではなく Weapon)</param>
    void ApplayDamage(float amount, IEntity instigator, Node causer);

    /// <summary>
    /// </summary>
    void ApplayKnockback(in Vector2 direction, float strength)
    {
    }
}