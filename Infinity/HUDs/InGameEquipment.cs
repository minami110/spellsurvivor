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
    private ProgressBar _progress = null!;

    public MinionCoreData ItemSettings { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = ItemSettings.Icon;
        _name.Text = ItemSettings.Name;
        var real = Main.GameMode.Minions[ItemSettings];

        var d1 = real.CoolDownLeft.Subscribe(this, (x, s) =>
        {
            var minion = Main.GameMode.Minions[s.ItemSettings];
            s._progress.MaxValue = minion.CoolDown;
            s._progress.Value = x;
        });
        Disposable.Combine(d1).AddTo(this);
    }
}