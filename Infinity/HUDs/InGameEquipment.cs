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
        _icon.Texture = Weapon.Config.Sprite;
        _name.Text = Weapon.Config.Name;
        _levelLabel.Text = $"Lv.{Weapon.State.Level.CurrentValue}";

        var d1 = Weapon.CoolDownLeft.Subscribe(this, (x, s) =>
        {
            var progress = s.GetNode<Range>("%CoolDownProgressBar");
            progress.MaxValue = Weapon.State.Cooldown.CurrentValue;
            progress.Value = x;
        });

        Disposable.Combine(d1).AddTo(this);
    }
}