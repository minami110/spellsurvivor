using System.Collections.Generic;
using fms;
using fms.Weapon;
using Godot;
using R3;

public partial class WeaponPositionAnimator : Node
{
    [Export(PropertyHint.Range, "0,100,1")]
    private float _radius = 30;

    private readonly List<Node2D> _weapons = new();

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // 兄弟階層の Tree 構造の変化を検知する
            // Note: Must be parent is MeMe
            GetParent().ChildOrderChangedAsObservable()
                .Subscribe(this, (_, self) => { self.RefreshWeapons(); })
                .AddTo(this);
        }
        else if (what == NotificationReady)
        {
            RefreshWeapons();
        }
    }

    private void RefreshWeapons()
    {
        _weapons.Clear();
        foreach (var n in this.GetSiblings())
        {
            if (n is not WeaponBase weapon)
            {
                continue;
            }

            if (weapon.AutoPosition)
            {
                _weapons.Add(weapon);
            }
        }

        if (_weapons.Count > 0)
        {
            UpdateSiblingWeaponsLocalPosition();
        }
    }

    private void UpdateSiblingWeaponsLocalPosition()
    {
        if (_weapons.Count == 0)
        {
            SetPhysicsProcess(false);
            return;
        }

        if (_radius <= 0)
        {
            return;
        }

        // Weapon の数に応じて N角形 になるように それぞれの Weapon の Position を更新する
        var angle = Mathf.Pi * 2 / _weapons.Count;
        var targetPosition = Vector2.Zero;
        for (var i = 0; i < _weapons.Count; i++)
        {
            if (_radius > 0)
            {
                targetPosition = new Vector2(
                    _radius * Mathf.Cos(angle * i),
                    _radius * Mathf.Sin(angle * i)
                );
            }

            // Weight に応じて移動する
            _weapons[i].Position = targetPosition;
        }
    }
}