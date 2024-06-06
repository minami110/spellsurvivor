using System;
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
        var d1 = Main.PlayerInventory.InHandMinionChanged.Subscribe(_ => OnChangedEquippedMinion());
        Disposable.Combine(d1).AddTo(this);
    }

    private void OnChangedEquippedMinion()
    {
        _bruiser.Hide();
        _duelist.Hide();
        _trickshot.Hide();

        var factions = Main.PlayerInventory.Factions;
        foreach (var (type, faction) in factions)
        {
            if (faction.Level == 0)
            {
                continue;
            }

            switch (type)
            {
                case FactionType.Bruiser:
                    UpdateLabel(_bruiser, nameof(Bruiser), faction);
                    break;
                case FactionType.Duelist:
                    UpdateLabel(_duelist, nameof(Duelist), faction);
                    break;
                case FactionType.Trickshot:
                    UpdateLabel(_trickshot, nameof(Trickshot), faction);
                    break;
                case FactionType.Invoker:
                    UpdateLabel(_invoker, nameof(Invoker), faction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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