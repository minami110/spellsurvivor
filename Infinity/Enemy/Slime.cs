using Godot;

namespace fms.Enemy;

/// <summary>
/// 近距離 (接触) ダメージを与える敵のベースクラス
/// </summary>
public partial class Slime : MeleeEnemy
{
    [Export]
    private PackedScene? _splitScene;

    [Export(PropertyHint.Range, "0,100,1")]
    private uint _splitCount = 4;

    private protected override void KillByDamage()
    {
        if (IsDead)
        {
            return;
        }

        if (_splitCount > 0u && _splitScene is not null)
        {
            // 自身の場所に分裂する
            for (var i = 0u; i < _splitCount; i++)
            {
                var slime = _splitScene.Instantiate<EnemyBase>();

                // Level はそのまま引き継ぐ 
                slime.Level = Level;

                // 自分の距離から 30px 以内に配置する
                var range = 30;
                slime.GlobalPosition =
                    GlobalPosition + new Vector2((float)GD.Randfn(0, range), (float)GD.Randfn(0, range));

                // 兄弟に配置する
                GetParent().CallDeferred(Node.MethodName.AddChild, slime);
            }
        }

        base.KillByDamage();
    }
}