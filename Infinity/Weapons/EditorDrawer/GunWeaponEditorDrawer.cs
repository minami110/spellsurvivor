using Godot;
using Vector2 = Godot.Vector2;

namespace fms.Weapon;

// Note: _draw を使用したいので Node2D を継承している
/// <summary>
/// <see cref="PiercingWeapon" />> の Editor での Drawer
/// </summary>
[Tool]
[GlobalClass]
public partial class GunWeaponEditorDrawer : Node2D
{
    [Export]
    private Color _minRangeColor = new(1, 0, 0, 0.2f);

    [Export]
    private Color _maxRangeColor = new(0, 1, 0);

    [Export]
    private Color _damageAreaColor = Colors.LawnGreen;

    private GodotObject? _weapon;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        _weapon ??= GetOwnerOrNull<GodotObject>();

        if (!(bool)_weapon.Get(WeaponBase.PropertyName.DrawDebugInfoInEditor))
        {
            // 何も呼び出さないことで描写を消す
            return;
        }

        DrawMinMaxRange();
        DrawBulletArea();
    }

    private void DrawBulletArea()
    {
        if (_weapon is null)
        {
            return;
        }

        // 当たり判定の攻撃判定を描写する
        var origin = (Vector2)_weapon.Get(AssaultRifle.PropertyName.Muzzle);
        var projectileSpeed = (float)_weapon.Get(AssaultRifle.PropertyName._speed);
        var projectileLife = (uint)_weapon.Get(AssaultRifle.PropertyName._life);
        var end = origin + new Vector2(projectileSpeed * (projectileLife / 60f), 0);

        // 短径の4辺をつなぐようなラインを描写する
        var color = _damageAreaColor;
        var width = 3f;
        DrawDashedLine(origin, end, color, width);
    }

    private void DrawMinMaxRange()
    {
        if (_weapon is null)
        {
            return;
        }

        var minRange = (float)_weapon.Get(PiercingWeapon.PropertyName._minRange);
        var maxRange = (float)_weapon.Get(PiercingWeapon.PropertyName._maxRange);

        // MinRange が手前に来るように描写する
        DrawCircle(Position, maxRange, _maxRangeColor, false);
        if (minRange > 0f)
        {
            DrawCircle(Position, minRange, _minRangeColor);
        }
    }
}