using Godot;

namespace fms;

public interface IEntity
{
    /// <summary>
    /// エフェクトを追加
    /// </summary>
    void AddEffect(string effectName);

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

    void GetProperty(string identifier)
    {
    }

    bool Kill()
    {
        return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="value"></param>
    void SetProperty(string identifier, Variant value)
    {
    }
}