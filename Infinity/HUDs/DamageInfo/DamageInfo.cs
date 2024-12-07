using Godot;
using R3;

namespace fms.HUD;

public static class TreeItemExtensions
{
    public static TreeItem? GetFirstChildByText(this TreeItem item, string name, int column = 0)
    {
        for (var i = 0; i < item.GetChildCount(); i++)
        {
            var child = item.GetChild(i);
            if (child.GetText(column) == name)
            {
                return child;
            }
        }

        return null;
    }
}

[Tool]
public partial class DamageInfo : Control
{
    private TreeItem _root = null!;
    private Tree _tree = null!;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            var tree = GetNode<Tree>("Tree");
            var root = tree.CreateItem();
            var child1 = tree.CreateItem(root);
            child1.SetText(0, "Sample");
            var child2 = tree.CreateItem(child1);
            child2.SetText(0, "WeaponFoo");
            child2.SetText(1, "200");
            child2.SetTextAlignment(1, HorizontalAlignment.Right);
            var child3 = tree.CreateItem(child1);
            child3.SetText(0, "WeaponBar");
            child3.SetText(1, "200");
            child3.SetSuffix(1, "dmg");
            child3.SetTextAlignment(1, HorizontalAlignment.Right);

            return;
        }

        _tree = GetNode<Tree>("Tree");

        // Create root
        _root = _tree.CreateItem();
        _tree.HideRoot = true;

        // Subscribe to enemy damage occurred
        StaticsManager.ReportedDamage.Subscribe(report =>
        {
            var parent = _root;

            // Player/TurretWeapon/Turret など Causer の 所属が確認できるパス が取得できる
            var causerPath = report.CauserPath;
            if (causerPath == string.Empty)
            {
                var item = parent.GetFirstChildByText("Anonymous");
                if (item == null)
                {
                    item = _tree.CreateItem(parent);
                    item.SetText(0, "Anonymous");
                    item.SetText(1, "0");
                }

                var value = item.GetText(1);
                var valueStr = value.ToFloat();
                valueStr += report.Amount;
                item.SetText(1, $"{valueStr:0}");

                return;
            }

            // これをそのまま Tree に表示する
            var path = causerPath.Split('/');
            foreach (var name in path)
            {
                var item = parent.GetFirstChildByText(name);
                if (item == null)
                {
                    item = _tree.CreateItem(parent);
                    item.SetText(0, name);
                    if (name is "Player" or "Enemy")
                    {
                        // Pass
                    }
                    else
                    {
                        item.SetText(1, "0");
                    }
                }

                parent = item;
            }

            // 第二要素にダメージ量を表示
            {
                var value = parent.GetText(1);
                var valueStr = value.ToFloat();
                valueStr += report.Amount;
                parent.SetText(1, $"{valueStr:0}");
            }
        }).AddTo(this);
    }
}