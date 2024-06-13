using System.Collections.Generic;
using fms.Faction;
using Godot;
using R3;

namespace fms;

public partial class ActiveFactionsManager : Node
{
    private readonly Dictionary<string, Label> _labels = new();

    public override void _Ready()
    {
        // 兄弟の Label を全部キャッシュしておく
        var nodes = this.GetSiblings();
        foreach (var node in nodes)
        {
            if (node is not Label label)
            {
                continue;
            }

            _labels.Add(label.Name, label);
        }

        // Player の子ノードが変わったら更新
        var player = this.GetPlayerNode();
        player.ChildOrderChangedAsObservable()
            .Subscribe(this, (_, state) => state.RefreshLabels())
            .AddTo(this);

        // 初回のラベル更新
        RefreshLabels();
    }

    private void HideAllLabel()
    {
        foreach (var (_, v) in _labels)
        {
            v.Hide();
        }
    }

    private void RefreshLabels()
    {
        HideAllLabel();

        var player = this.GetPlayerNode();
        foreach (var faction in player.GetChildren())
        {
            if (faction is not FactionBase factionBase)
            {
                continue;
            }

            if (factionBase.Level == 0)
            {
                continue;
            }

            var typeName = factionBase.GetType().Name;
            if (!_labels.TryGetValue(typeName, out var label))
            {
                continue;
            }

            UpdateLabel(label, typeName, factionBase);
        }
    }

    private static void UpdateLabel(Label characterLabel, string factionName, FactionBase faction)
    {
        characterLabel.Text = $"{factionName} Lv.{faction.Level}";
        characterLabel.Modulate = faction.IsActiveAnyEffect ? new Color(1, 1, 1) : new Color(1, 1, 1, 0.3f);
        characterLabel.Show();
    }
}