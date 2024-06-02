using System;
using System.Linq;
using fms.Weapon;
using Godot;
using R3;

namespace fms;

public partial class InGameEquipment : VBoxContainer
{
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    [Export]
    private Label _levelLabel = null!;

    [Export]
    private ProgressBar _progress = null!;

    public WeaponBase Weapon { get; set; } = null!;

    public override void _Ready()
    {
        var inventoryData = Main.PlayerInventory.Minions.FirstOrDefault(m => m.Id == Weapon.Id);
        if (inventoryData == null)
        {
            throw new NotImplementedException("Minion が見つかりませんでした");
        }

        _icon.Texture = inventoryData.Sprite;
        _name.Text = inventoryData.Name;

        _levelLabel.Text = $"Lv.{Weapon.MinionLevel}";

        var d1 = Weapon.CoolDownLeft.Subscribe(this, (x, s) =>
        {
            s._progress.MaxValue = Weapon.CoolDown;
            s._progress.Value = x;
        });

        Disposable.Combine(d1).AddTo(this);
    }
}