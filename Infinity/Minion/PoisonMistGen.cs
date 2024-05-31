using fms.Faction;
using fms.Projectile;
using Godot;

namespace fms.Minion;

public partial class PoisonMistGen : MinionBase
{
    [ExportGroup("Internal Reference")]
    [Export]
    private PackedScene _bulletPackedScene = null!;

    [Export]
    private Node _bulletSpawnNode = null!;

    // この Minion が所属する Faction の一覧
    private static readonly FactionBase[] _factions =
    {
        new Bruiser(),
        new Duelist(),
        new Knight(),
        new Trickshot()
    };

    private int _trickshotBounceCount;
    private float _trickshotBounceDamageMultiplier;

    private protected override int BaseCoolDownFrame => 300;

    public override FactionBase[] Factions => _factions;

    private protected override void DoAttack()
    {
        SpawnBullet(Level.CurrentValue);
    }

    private void SpawnBullet(int level)
    {
        if (level == 1)
        {
            var bullet = _bulletPackedScene.Instantiate<PoisonMist>();
            {
                bullet.InitialPosition = Main.PlayerGlobalPosition;
                bullet.CoolDown = 10; // 10 フレームに一回敵にダメージ
                bullet.BaseDamage = 1; // 10 フレームに1ダメージ
                bullet.Radius = 100; // 100 ピクセル以内の敵にダメージ
                bullet.LifeFrame = 300; // 300 フレーム後に消滅
            }
            _bulletSpawnNode.AddChild(bullet);
        }
        else if (level == 2)
        {
            var bullet = _bulletPackedScene.Instantiate<PoisonMist>();
            {
                bullet.InitialPosition = Main.PlayerGlobalPosition;
                bullet.CoolDown = 10; // 10 フレームに一回敵にダメージ
                bullet.BaseDamage = 1; // 10 フレームに1ダメージ
                bullet.Radius = 200; // 200 ピクセル以内の敵にダメージ
                bullet.LifeFrame = 300; // 300 フレーム後に消滅
            }
            _bulletSpawnNode.AddChild(bullet);
        }
        else if (level == 3)
        {
            var bullet = _bulletPackedScene.Instantiate<PoisonMist>();
            {
                bullet.InitialPosition = Main.PlayerGlobalPosition;
                bullet.CoolDown = 10; // 10 フレームに一回敵にダメージ
                bullet.BaseDamage = 1; // 10 フレームに1ダメージ
                bullet.Radius = 300; // 200 ピクセル以内の敵にダメージ
                bullet.LifeFrame = 400; // 400 フレーム後に消滅
            }
            _bulletSpawnNode.AddChild(bullet);
        }
    }
}