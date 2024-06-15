using System.Collections.Generic;
using fms;
using Godot;
using R3;

public partial class WeaponPositionAnimator : Node
{
    private readonly List<Node2D> _weapons = new();

    public override void _EnterTree()
    {
        // Note: Must be parent is MeMe
        var parent = GetParent();
        parent.ChildOrderChangedAsObservable()
            .Subscribe(x => { RefreshWeapons(); })
            .AddTo(this);
    }

    public override void _Ready()
    {
        RefreshWeapons();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_weapons.Count == 0)
        {
            SetPhysicsProcess(false);
            return;
        }

        var center = GetParent<Node2D>().GlobalPosition;

        var radius = 10 + _weapons.Count * 3f;

        // Weapon の数に応じて N角形 になるように それぞれの Weapon の Position を更新する
        var angle = Mathf.Pi * 2 / _weapons.Count;
        for (var i = 0; i < _weapons.Count; i++)
        {
            var weapon = _weapons[i];
            var pos = new Vector2(
                center.X + radius * Mathf.Cos(angle * i),
                center.Y + radius * Mathf.Sin(angle * i)
            );
            // Weight に応じて移動する
            weapon.GlobalPosition = pos;
        }
    }

    private void RefreshWeapons()
    {
        _weapons.Clear();
        var sib = this.GetSiblings();
        foreach (var node in sib)
        {
            if (node is not Node2D n2)
            {
                continue;
            }

            if (node.IsInGroup(Constant.GroupNameWeapon))
            {
                _weapons.Add(n2);
            }
        }

        if (_weapons.Count > 0)
        {
            SetPhysicsProcess(true);
        }
    }
}