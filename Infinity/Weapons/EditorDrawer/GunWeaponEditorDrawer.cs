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

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        SetProcess(false);
        QueueFree();
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
        DrawBulletLine();
    }

    private void DrawBulletLine()
    {
        if (_weapon is null)
        {
            return;
        }

        // 当たり判定の攻撃判定を描写する

        // Note: Export されてない Property は評価できないのでこっちで Muzzle を取得
        var muzzle = (Node2D?)_weapon.Get(AssaultRifle.PropertyName._muzzle);
        var start = muzzle?.GlobalPosition ?? ((Node2D)_weapon).GlobalPosition;

        var projectileSpeed = (float)_weapon.Get(AssaultRifle.PropertyName._speed);
        var projectileLife = (uint)_weapon.Get(AssaultRifle.PropertyName._life);
        var length = projectileSpeed * (projectileLife / 60f);
        var end = start + new Vector2(length, 0);
        var color = _damageAreaColor;
        var dotWidth = 3f;
        DrawDashedLine(start, end, color, dotWidth);
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