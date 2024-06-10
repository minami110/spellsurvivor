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

    [Export]
    private Label _invoker = null!;

    public override void _Ready()
    {
        HideAllLabel();
        GetTree().TreeChangedAsObservable().Subscribe(OnChangedTree).AddTo(this);
    }

    private void HideAllLabel()
    {
        _bruiser.Hide();
        _duelist.Hide();
        _trickshot.Hide();
        _invoker.Hide();
    }

    private void OnChangedTree(Unit _)
    {
        HideAllLabel();

        var factions = GetTree().GetNodesInGroup(Constant.GroupNameFaction);

        foreach (var faction in factions)
        {
            if (faction is not FactionBase factionBase)
            {
                continue;
            }

            if (factionBase.Level == 0)
            {
                continue;
            }

            var type = factionBase.GetType();
            if (type == typeof(Bruiser))
            {
                UpdateLabel(_bruiser, nameof(Bruiser), factionBase);
            }
            else if (type == typeof(Duelist))
            {
                UpdateLabel(_duelist, nameof(Duelist), factionBase);
            }
            else if (type == typeof(Trickshot))
            {
                UpdateLabel(_trickshot, nameof(Trickshot), factionBase);
            }
            else if (type == typeof(Invoker))
            {
                UpdateLabel(_invoker, nameof(Invoker), factionBase);
            }
        }
    }

    private static void UpdateLabel(Label characterLabel, string factionName, FactionBase faction)
    {
        characterLabel.Text = $"{factionName} Lv.{faction.Level}";
        characterLabel.Modulate = faction.IsActiveAnyEffect ? new Color(1, 1, 1) : new Color(1, 1, 1, 0.3f);
        characterLabel.Show();
    }
}