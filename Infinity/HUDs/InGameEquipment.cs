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

    public MinionCoreData MinionCoreData { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = MinionCoreData.Icon;
        _name.Text = MinionCoreData.Name;
        var minion = Main.PlayerInventory.EquippedMinions[MinionCoreData];

        _levelLabel.Text = $"Lv.{minion.Level}";

        var d1 = minion.CoolDownLeft.Subscribe(this, (x, s) =>
        {
            var m = Main.PlayerInventory.EquippedMinions[s.MinionCoreData];
            s._progress.MaxValue = m.CoolDown;
            s._progress.Value = x;
        });
        Disposable.Combine(d1).AddTo(this);
    }
}