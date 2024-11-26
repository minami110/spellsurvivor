using fms.Projectile;
using Godot;
using Godot.Collections;

namespace fms.Weapon;

public partial class ForwardKnife : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 500f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120u;

    [ExportGroup("For Debugging")]
    // 反射回数
    [Export]
    private int BounceCount { get; set; }

    [Export]
    // 反射時のダメージ倍率
    private float BounceDamageMul { get; set; }

    private Vector2 _latestDirection = Vector2.Right;

    private Vector2 _prevGlobalPosition;

    public override void _Process(double delta)
    {
        var currentGlobalPosition = GlobalPosition;
        var direction = currentGlobalPosition - _prevGlobalPosition;
        if (direction.LengthSquared() > 0)
        {
            _latestDirection = direction;
        }

        _prevGlobalPosition = currentGlobalPosition;
    }

    private protected override void OnCoolDownCompleted(uint level)
    {
        switch (level)
        {
            // Level 1 は1つの弾をだす
            case 1:
            {
                SpawnBullet(GlobalPosition);
                break;
            }
            // Level 2 は2つの弾をだす
            case 2:
            {
                SpawnBullet(GlobalPosition, 10f);
                SpawnBullet(GlobalPosition, -10f);
                break;
            }
            // Level 3 以上は同じ
            default:
            {
                SpawnBullet(GlobalPosition, 20f);
                SpawnBullet(GlobalPosition, 0f, 10f);
                SpawnBullet(GlobalPosition, -20f);
                break;
            }
        }

        RestartCoolDown();
    }

    private protected override void OnUpdateAnyAttribute(Dictionary<string, Variant> attributes)
    {
        BounceCount = 0;
        BounceDamageMul = 0f;

        if (attributes.TryGetValue(WeaponAttributeNames.BounceCount, out var trickShotCount))
        {
            BounceCount = (int)trickShotCount;
        }

        if (attributes.TryGetValue(WeaponAttributeNames.BounceDamageRate, out var trickShotDamageMul))
        {
            BounceDamageMul = (float)trickShotDamageMul;
        }
    }

    // 引数の Projectile に Trickshot Mod を登録する
    private static void AddTrickshotMod(BaseProjectile parent, Dictionary payload)
    {
        parent.AddChild(new TrickshotMod
        {
            Next = SpawnNextProjectile,
            When = WhyDead.CollidedWithAny,
            SearchRadius = 200, // Trickshot 実行時の敵検索範囲,
            Payload = payload
        });
    }

    private Vector2 GetAimDirection()
    {
        // コントローラーがつながっている場合は, 右スティックの入力方法を返す
        if (Input.GetConnectedJoypads().Count > 0)
        {
            var deadZone = 0.2f;
            // ToDo: InputAction 使用したほうがいいかも
            var joyX = Input.GetJoyAxis(0, JoyAxis.RightX);
            if (joyX < deadZone && joyX > -deadZone)
            {
                joyX = 0;
            }

            var joyY = Input.GetJoyAxis(0, JoyAxis.RightY);
            if (joyY < deadZone && joyY > -deadZone)
            {
                joyY = 0;
            }

            var joy = (Vector2.Right * joyX + Vector2.Down * joyY).Normalized();

            if (joy.LengthSquared() > 0)
            {
                return joy;
            }
        }

        // コントローラーがつながっているが, 右スティックを倒していない, あるいはコントローラーがつながっていない場合は, 最後に動いた方向を返す
        return _latestDirection;
    }

    private void SpawnBullet(in Vector2 center, float xOffset = 0f, float yOffset = 0f)
    {
        // Main の Projectile
        var prjMain = _projectile.Instantiate<BaseProjectile>();
        {
            prjMain.Damage = State.Damage.CurrentValue;
            prjMain.Knockback = State.Knockback.CurrentValue;
            prjMain.LifeFrame = _life;

            // Get Player's aim direction
            prjMain.ConstantForce = GetAimDirection().Normalized() * _speed;
        }

        // Trickshot が有効な場合は Mod を登録する
        if (BounceCount > 0 || BounceDamageMul > 0f)
        {
            var payload = new Dictionary
            {
                // ToDo: Trickshot に Effect 適用後の値をのせてもいい?
                { "BaseDamage", State.Damage.CurrentValue },
                { "Knockback", State.Knockback.CurrentValue },
                { "Speed", _speed },
                { "Life", _life },
                { "TrickShotCount", BounceCount },
                { "TrickShotDamageMul", BounceDamageMul },
                { "ProjectileScene", _projectile }
            };

            AddTrickshotMod(prjMain, payload);
        }

        var spawnPos = center + GlobalTransform.Y * xOffset + GlobalTransform.X * yOffset;
        AddProjectile(prjMain, spawnPos);
    }

    // ヒット時に Mod から呼ばれるコールバック, 次の弾を生成する 
    private static BaseProjectile SpawnNextProjectile(Dictionary payload)
    {
        var prj = ((PackedScene)payload["ProjectileScene"]).Instantiate<BaseProjectile>();

        prj.Position = (Vector2)payload["DeadPosition"];
        prj.Damage = (float)payload["BaseDamage"] * (float)payload["TrickShotDamageMul"];
        prj.Knockback = (uint)payload["Knockback"];
        prj.LifeFrame = (uint)payload["Life"];
        prj.ConstantForce = (Vector2)payload["Direction"] * (float)payload["Speed"];

        if ((int)payload["Iter"] < (int)payload["TrickShotCount"])
        {
            AddTrickshotMod(prj, payload);
        }

        return prj;
    }
}