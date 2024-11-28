using Godot;
using R3;

namespace fms.HUD;

/// <summary>
/// Shop で販売しているアイテム の HUD, Shop により自動で生成される
/// </summary>
[Tool]
public partial class SellingWeaponCardButton : FmsButton
{
    [Export]
    private string Title
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                GetNode<Label>("%Title").Text = value;
            }
        }
    } = string.Empty;

    [Export(PropertyHint.Range, "1,1000")]
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
    }

    [Export(PropertyHint.Range, "1,5")]
    private uint Tier
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                UpdateTier();
            }
        }
    } = 1u;

    [Export]
    private Texture2D? Sprite
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                GetNode<TextureRect>("%CardSprite").Texture = value;
            }
        }
    }

    [Export]
    private string Faction0
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                UpdateFaction();
            }
        }
    } = string.Empty;

    [Export]
    private string Faction1
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                UpdateFaction();
            }
        }
    } = string.Empty;

    [Export]
    private string Faction2
    {
        get;
        set
        {
            field = value;
            if (IsNodeReady())
            {
                UpdateFaction();
            }
        }
    } = string.Empty;


    private bool _isSoldOut;

    public WeaponCard WeaponCard { get; internal set; } = null!;

    public override void _Ready()
    {
        GetNode<Label>("%Title").Text = Title;
        GetNode<Label>("%Price").Text = $"{Price}";
        GetNode<TextureRect>("%CardSprite").Texture = Sprite;
        UpdateTier();
        UpdateFaction();

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
        Disabled = money < Price;
    }

    private void OnPressed()
    {
        // GameMode に通知する
        var player = this.GetPlayerNode();
        Main.Shop.BuyWeaponCard((IEntity)player, WeaponCard);
        SoldOut();
    }

    private void SoldOut()
    {
        _isSoldOut = true;
        Disabled = true;
    }

    private void UpdateFaction()
    {
        GetNode<FactionInfo>("%Faction0").Faction = Faction0;
        GetNode<FactionInfo>("%Faction1").Faction = Faction1;
        GetNode<FactionInfo>("%Faction2").Faction = Faction2;
    }

    private void UpdateTier()
    {
        var c = GetNode<ColorRect>("%BGSprite");
        c.Color = Tier switch
        {
            1u => FmsColors.TierCommon,
            2u => FmsColors.TierUncommon,
            3u => FmsColors.TierRare,
            4u => FmsColors.TierEpic,
            5u => FmsColors.TierLegendary,
            _ => c.Color
        };
    }
}