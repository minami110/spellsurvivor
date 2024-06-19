using System;
using fms.Weapon;
using Godot;
using R3;
using Range = Godot.Range;

namespace fms;

public partial class InGameEquipment : VBoxContainer
{
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    [Export]
    private Label _levelLabel = null!;

    public WeaponBase Weapon { get; set; } = null!;

    public override void _Ready()
    {
        Minion? targetMinion = null;
        var nodes = GetTree().GetNodesInGroup(Constant.GroupNameMinion);
        foreach (var node in nodes)
        {
            if (node is not Minion minion)
            {
                continue;
            }

            if (minion.Weapon != Weapon)
            {
                continue;
            }

            targetMinion = minion;
            break;
        }

        if (targetMinion is null)
        {
            throw new ApplicationException($"Minion not found for {Weapon}");
        }

        _icon.Texture = targetMinion.Sprite;
        _name.Text = targetMinion.FriendlyName;
        _levelLabel.Text = $"Lv.{Weapon.Level}";

        var d1 = Weapon.CoolDownLeft.Subscribe(this, (x, s) =>
        {
            var progress = s.GetNode<Range>("%CoolDownProgressBar");
            progress.MaxValue = Weapon.SolvedCoolDownFrame;
            progress.Value = x;
        });

        Disposable.Combine(d1).AddTo(this);
    }
}