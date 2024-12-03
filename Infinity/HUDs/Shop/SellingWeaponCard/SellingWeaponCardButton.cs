using fms.Faction;
using Godot;
using Godot.Collections;
using R3;

namespace fms.HUD;

/// <summary>
/// Shop で販売しているアイテム の HUD, Shop により自動で生成される
/// </summary>
public partial class SellingWeaponCardButton : FmsButton
{
    [Export]
    private uint Index { get; set; }

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
            UpdatePriceUi();
        }
    }

    [Export]
    private TierType Tier
    {
        get;
        set
        {
            field = value;
            UpdateTierUi();
        }
    } = TierType.Common;

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
    private FactionType Faction
    {
        get;
        set
        {
            field = value;
            UpdateFactions();
        }
    }

    private readonly Subject<Unit> _requestHideInfo = new();

    private readonly Subject<Dictionary> _requestShowInfo = new();

    private int _clickCount;

    internal WeaponCard? WeaponCard
    {
        private get;
        set
        {
            field = value;
            OnUpdateWeaponCard();
        }
    }

    private bool IsSoldOut => WeaponCard is null;

    public Observable<Dictionary> RequestShowInfo => _requestShowInfo;
    public Observable<Unit> RequestHideInfo => _requestHideInfo;

    public override void _Ready()
    {
        GetNode<Label>("%Title").Text = Title;
        GetNode<TextureRect>("%CardSprite").Texture = Sprite;
        UpdatePriceUi();
        UpdateTierUi();
        UpdateFactions();

        // Subscribe 
        var d1 = this.PressedAsObservable().Subscribe(this, (_, s) => s.OnPressed());
        var playerState = (EntityState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        var d2 = playerState.Money.ChangedCurrentValue.Subscribe(this, (x, s) => s.OnChangedPlayerMoney(x));

        // マウスが侵入したら Tooltip を表示
        var d3 = this.FocusEnteredAsObservable()
            .Subscribe(_ =>
            {
                if (WeaponCard is not null)
                {
                    var dict = new Dictionary
                    {
                        { "Title", WeaponCard.FriendlyName },
                        { "Description", WeaponCard.DescriptionForShop },
                        { "Price", WeaponCard.Price },
                        { "Tier", (uint)WeaponCard.Tier }
                    };
                    _requestShowInfo.OnNext(dict);
                }
            });

        var d4 = ToastManager.Instance.FocusEntered
            .Subscribe(key =>
            {
                if (!key.StartsWith(FocusKey))
                {
                    _clickCount = 0;
                    _requestHideInfo.OnNext(Unit.Default);
                }
            });

        Disposable.Combine(d1, d2, d3, d4, _requestShowInfo, _requestHideInfo).AddTo(this);
    }

    private void OnChangedPlayerMoney(uint money)
    {
        Disabled = money < Price;
    }

    private void OnPressed()
    {
        if (IsSoldOut)
        {
            return;
        }

        // Focus中にクリックされたとき (操作的にはダブルクリック) に購入する
        if (_clickCount < 1)
        {
            _clickCount++;
            return;
        }

        // GameMode に通知する
        var player = this.GetPlayerNode();
        Main.Shop.BuyWeaponCard((IEntity)player, (int)Index);
    }

    private void OnUpdateWeaponCard()
    {
        // null なら売り切れの見た目にする
        if (WeaponCard is null)
        {
            Title = "Sold Out";
            Price = 0;
            Sprite = null;
            Tier = 0u;
            Faction = 0u;
            _requestHideInfo.OnNext(Unit.Default);
        }
        else
        {
            // Initialize
            Title = WeaponCard.FriendlyName;
            Price = WeaponCard.Price;
            Sprite = WeaponCard.Sprite;
            Tier = WeaponCard.Tier;
            Faction = WeaponCard.Faction;
        }
    }

    private void UpdateFactions()
    {
        if (!IsNodeReady())
        {
            return;
        }

        var allFactions = FactionUtil.GetFactionTypes();
        var index = 0;

        foreach (var f in allFactions)
        {
            if (index >= 3)
            {
                break;
            }

            if (!Faction.HasFlag(f))
            {
                continue;
            }

            GetNode<FacionLabel>($"%FactionLabel{index}").Faction = f;

            index++;
        }

        if (index < 3)
        {
            for (var i = index; i < 3; i++)
            {
                GetNode<FacionLabel>($"%FactionLabel{i}").Faction = 0u;
            }
        }
    }

    private void UpdatePriceUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Price > 0u)
        {
            GetNode<Control>("%CoinSprite").Show();
            GetNode<Control>("%Price").Show();
            GetNode<Label>("%Price").Text = $"{Price}";
        }
        else
        {
            GetNode<Control>("%CoinSprite").Hide();
            GetNode<Control>("%Price").Hide();
        }
    }

    private void UpdateTierUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        var c = GetNode<ColorRect>("%BGSprite");
        c.Color = Tier switch
        {
            TierType.Common => FmsColors.TierCommon,
            TierType.Uncommon => FmsColors.TierUncommon,
            TierType.Rare => FmsColors.TierRare,
            TierType.Epic => FmsColors.TierEpic,
            TierType.Legendary => FmsColors.TierLegendary,
            _ => c.Color
        };
    }
}