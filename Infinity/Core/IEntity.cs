using Godot;
using R3;

namespace fms;

public interface IEntity
{
    /// <summary>
    /// ダメージを与える
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="instigator">ダメージを引き起こした Entity (例: プレイヤー)</param>
    /// <param name="causer">実際に被害を引き起こした主体 例: 手榴弾 Prj ではなく Weapon)</param>
    void ApplayDamage(float amount, IEntity instigator, Node causer);
}
