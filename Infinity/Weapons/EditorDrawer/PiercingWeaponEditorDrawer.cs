using Godot;
using Vector2 = Godot.Vector2;

namespace fms.Weapon;

// Note: _draw を使用したいので Node2D を継承している
/// <summary>
/// <see cref="PiercingWeapon" />> の Editor での Drawer
/// </summary>
[Tool]
[GlobalClass]
public partial class PiercingWeaponEditorDrawer : Node2D
{
    [Export]
    private Color _minRangeColor = new(1, 0, 0, 0.2f);

    [Export]
    private Color _maxRangeColor = new(0, 1, 0);

    [Export]
    private Color _damageAreaColor = Colors.LawnGreen;

    private GodotObject? _hocho;

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
        _hocho ??= GetOwnerOrNull<GodotObject>();

        if (!(bool)_hocho.Get(WeaponBase.PropertyName.DrawDebugInfoInEditor))
        {
            // 何も呼び出さないことで描写を消す
            return;
        }

        DrawMinMaxRange();
        DrawStaticDamageArea();
    }

    private void DrawMinMaxRange()
    {
        if (_hocho is null)
        {
            return;
        }

        var minRange = (float)_hocho.Get(PiercingWeapon.PropertyName._minRange);
        var maxRange = (float)_hocho.Get(PiercingWeapon.PropertyName._maxRange);

        // MinRange が手前に来るように描写する
        DrawCircle(Position, maxRange, _maxRangeColor, false);
        if (minRange > 0f)
        {
            DrawCircle(Position, minRange, _minRangeColor);
        }
    }

    private void DrawStaticDamageArea()
    {
        if (_hocho is null)
        {
            return;
        }

        // 当たり判定の攻撃判定を描写する
        var cs = GetNode<CollisionShape2D>("%StaticDamage/CollisionShape2D");
        var csPos = cs.Position;
        var size = ((RectangleShape2D)cs.Shape).Size;

        // 矩形の描写
        var preAttackDistance = (uint)_hocho.Get(PiercingWeapon.PropertyName._preAttackDistance);
        var pushDistance = (uint)_hocho.Get(PiercingWeapon.PropertyName._pushDistance);

        // Push 時の当たり判定の描写
        var origin = csPos - size / 2f;
        origin -= new Vector2(preAttackDistance, 0);
        size += new Vector2(preAttackDistance + pushDistance, 0);

        // 短径の4辺をつなぐようなラインを描写する
        var color = _damageAreaColor;
        var width = 0.5f;
        DrawDashedLine(origin + new Vector2(0, 0), origin + new Vector2(size.X, 0), color, width);
        DrawDashedLine(origin + new Vector2(size.X, 0), origin + size, color, width);
        DrawDashedLine(origin + size, origin + new Vector2(0, size.Y), color, width);
        DrawDashedLine(origin + new Vector2(0, size.Y), origin + new Vector2(0, 0), color, width);
    }
}