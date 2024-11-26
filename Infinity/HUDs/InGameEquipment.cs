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
            // Note: シナジーなどから生成された特殊な武器の場合は、対象のミニオンが存在しないことがある
            // このような場合は描写を行わない
            Hide(); // ToDo: めっちゃ適当な処理, Minion ナシ とか ショップ排出ナシの武器 みたいなもっと上位な仕組みを用意する

            return;
        }

        _icon.Texture = targetMinion.Sprite;
        _name.Text = targetMinion.FriendlyName;
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