using fms.Faction;
using Godot;
using R3;

namespace fms;

public partial class ActiveFactionsManager : Node
{
    [Export]
    private Label _bruiser = null!;

    [Export]
    private Label _duelist = null!;

    [Export]
    private Label _trickshot = null!;

    public override void _Ready()
    {
        var d1 = Main.GameMode.ChangedEquippedMinion.Subscribe(_ => OnChangedEquippedMinion());
        Disposable.Combine(d1).AddTo(this);
    }

    private void OnChangedEquippedMinion()
    {
        _duelist.Hide();
        _trickshot.Hide();

        var factions = Main.GameMode.FactionMap;
        foreach (var (type, faction) in factions)
        {
            if (faction.Level == 0)
            {
                continue;
            }

            if (type == typeof(Bruiser))
            {
                UpdateLabel(_bruiser, nameof(Bruiser), faction);
            }

            if (type == typeof(Duelist))
            {
                UpdateLabel(_duelist, nameof(Duelist), faction);
            }
            else if (type == typeof(Trickshot))
            {
                UpdateLabel(_trickshot, nameof(Trickshot), faction);
            }
        }
    }

    private void UpdateLabel(Label characterLabel, string characterName, FactionBase faction)
    {
        characterLabel.Show();
        characterLabel.Text = $"{characterName} Lv.{faction.Level}";
        characterLabel.Modulate = faction.IsAnyEffectActive ? new Color(1, 1, 1) : new Color(1, 1, 1, 0.3f);
    }
}