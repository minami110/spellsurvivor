using fms;
using Godot;
using R3;

public partial class InGameEquipment : VBoxContainer
{
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    [Export]
    private ProgressBar _progress = null!;

    public ShopItemSettings ItemSettings { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = ItemSettings.Icon;
        _name.Text = ItemSettings.Name;

        var real = Main.GameMode.Minions[ItemSettings];
        var d1 = real.CoolDownLeft.Subscribe(x =>
        {
            _progress.MaxValue = real.MaxCoolDown.CurrentValue;
            _progress.Value = x;
        });
        Disposable.Combine(d1).AddTo(this);
    }
}