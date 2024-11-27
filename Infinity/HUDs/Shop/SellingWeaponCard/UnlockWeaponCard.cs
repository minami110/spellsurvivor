using System;
using Godot;
using R3;

namespace fms.HUD;

[Tool]
public partial class UnlockWeaponCard : Button
{
    [Export]
    private uint Price
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                GetNode<Label>("%Price").Text = $"{value}";
            }
        }
    } = 5u;

    public override void _Ready()
    {
        GetNode<Label>("%Price").Text = $"{Price}";

        if (Engine.IsEditorHint())
        {
            return;
        }

        // Subscribe 
        var d1 = this.PressedAsObservable().Subscribe(this, (_, s) => s.OnPressed());
        var playerState = (EntityState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        var d2 = playerState.Money.ChangedCurrentValue.Subscribe(this, (x, s) => s.OnChangedPlayerMoney(x));

        Disposable.Combine(d1, d2).AddTo(this);
    }

    private void OnChangedPlayerMoney(uint money)
    {
        if (money < Price)
        {
            Modulate = new Color(1, 0, 0);
            Disabled = true;
            MouseDefaultCursorShape = CursorShape.Arrow;
        }
        else
        {
            Modulate = new Color(0, 1, 0);
            Disabled = false;
            MouseDefaultCursorShape = CursorShape.PointingHand;
        }
    }

    private void OnPressed()
    {
        throw new NotImplementedException();
    }
}