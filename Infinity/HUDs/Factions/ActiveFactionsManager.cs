using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class ActiveFactionsManager : Node
{
    [Export]
    private uint _maxSlotCount = 10;

    public override void _Ready()
    {
        var player = this.GetPlayerNode();
        player.ChildOrderChangedAsObservable()
            .Subscribe(this, (_, self) => self.UpdateUi())
            .AddTo(this);

        for (var i = 0; i < _maxSlotCount; i++)
        {
            var label = GetNode<ActiveFactionLabel>($"%FactionLabel{i}");
            label.RequestShowInfo
                .Subscribe(ShowFactionInfo)
                .AddTo(this);
            label.RequestHideInfo
                .Subscribe(HideFactionInfo)
                .AddTo(this);
        }

        UpdateUi();
    }

    private void HideFactionInfo(Unit _)
    {
        GetNode<FactionInfo>("%FactionInfo").Faction = 0u;
    }

    private void ShowFactionInfo(FactionType factionType)
    {
        GetNode<FactionInfo>("%FactionInfo").Faction = factionType;
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        // Player の 子にある FactionBase 継承クラス
        var index = 0;
        var nodes = this.GetPlayerNode().GetChildren();

        foreach (var n in nodes)
        {
            if (index >= _maxSlotCount)
            {
                break;
            }

            if (n is not FactionBase factionBase)
            {
                continue;
            }

            var label = GetNode<ActiveFactionLabel>($"%FactionLabel{index}");
            label.Faction = factionBase;
            index++;
        }

        for (var i = index; i < _maxSlotCount; i++)
        {
            var label = GetNode<ActiveFactionLabel>($"%FactionLabel{i}");
            label.Faction = null;
        }
    }
}