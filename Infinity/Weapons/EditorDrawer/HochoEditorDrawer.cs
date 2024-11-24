using Godot;

namespace fms.Weapon;

/// <summary>
/// Editor 専用の Drawer
/// </summary>
[Tool]
[GlobalClass]
public partial class HochoEditorDrawer : Node2D
{
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
        if ((bool)_hocho.Get(WeaponBase.PropertyName.DrawDebugInfoInEditor))
        {
            DrawMinMaxRange();
            DrawStaticDamageArea();
        }
        // 何も呼び出さないことで描写を消す
    }

    private void DrawMinMaxRange()
    {
        if (_hocho is null)
        {
            return;
        }

        var minRange = (float)_hocho.Get(Hocho.PropertyName._minRange);
        var maxRange = (float)_hocho.Get(Hocho.PropertyName._maxRange);

        var minRangeColor = new Color(1, 0, 0, 0.5f);
        var maxRangeColor = new Color(0, 1, 0, 0.5f);

        // MinRange が手前に来るように描写する
        DrawCircle(Position, maxRange, maxRangeColor);
        if (minRange > 0f)
        {
            DrawCircle(Position, minRange, minRangeColor);
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
        var preAttackDistance = (uint)_hocho.Get(Hocho.PropertyName._preAttackDistance);
        var pushDistance = (uint)_hocho.Get(Hocho.PropertyName._pushDistance);
        ;

        // Push 時の当たり判定の描写
        var origin = csPos - size / 2f;
        origin -= new Vector2(preAttackDistance, 0);
        size += new Vector2(preAttackDistance + pushDistance, 0);
        DrawRect(new Rect2(origin, size), new Color(0, 0, 1));
    }
}