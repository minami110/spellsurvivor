using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class OwnedWeaponInfo : Control
{
    public WeaponBase? Weapon
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    public override void _Ready()
    {
        // Faction Toast
        var count = 3;
        for (var i = 0; i < count; i++)
        {
            var factionLabel = GetNode<FacionLabel>($"%FactionLabel{i}");
            factionLabel.RequestShowInfo
                .Subscribe(ShowFactionInfo)
                .AddTo(this);

            factionLabel.RequestHideInfo
                .Subscribe(HideFactionInfo)
                .AddTo(this);
        }

        // Weapon Stat Toast
        var damage = GetNode<WeaponStatLabel>("%Damage");
        var attackSpeed = GetNode<WeaponStatLabel>("%AttackSpeed");
        var knockback = GetNode<WeaponStatLabel>("%Knockback");
        var lifeSteal = GetNode<WeaponStatLabel>("%Lifesteal");
        foreach (var n in new[] { damage, attackSpeed, knockback, lifeSteal })
        {
            n.RequestShowInfo
                .Subscribe(ShowStatInfo)
                .AddTo(this);
            n.RequestHideInfo
                .Subscribe(HideStatInfo)
                .AddTo(this);
        }

        // Sell Button
        var sellButton = GetNode<BaseButton>("%SellButton");
        sellButton.PressedAsObservable()
            .Subscribe(_ =>
            {
                if (Weapon is null)
                {
                    return;
                }

                // ToDo: Card を検索しているが, 将来的に Card と Weapon は統合する
                var player = this.GetPlayerNode();
                var id = Weapon.Config.Id;
                WeaponCard? targetCard = null;
                foreach (var n in player.GetChildren())
                {
                    if (n is WeaponCard card)
                    {
                        if (card.Id == id)
                        {
                            targetCard = card;
                            break;
                        }
                    }
                }

                if (targetCard is not null)
                {
                    Main.Shop.SellWeaponCard((IEntity)player, targetCard);
                    ToastManager.Instance.CommitFocusEntered("Dummy"); // 自身を閉じるため新しい Focus を送る
                }
            })
            .AddTo(this);

        UpdateUi();
    }

    private void HideFactionInfo(Unit _)
    {
        GetNode<FactionInfo>("%FactionInfo").Faction = 0u;
    }

    private void HideStatInfo(Unit _)
    {
        var statInfo = GetNode<WeaponStatInfo>("%WeaponStatInfo");
        statInfo.StatType = 0u;
    }

    private void ShowFactionInfo(FactionType faction)
    {
        var factionInfo = GetNode<FactionInfo>("%FactionInfo");

        if (faction != 0u)
        {
            var firstItem = GetNode<Control>("%FactionLabel0");
            var y = firstItem.GlobalPosition.Y;
            factionInfo.GlobalPosition = new Vector2(factionInfo.GlobalPosition.X, y);
        }

        factionInfo.Faction = faction;
    }

    private void ShowStatInfo(WeaponStatusType statType)
    {
        // Toast の位置を調整
        var statInfo = GetNode<WeaponStatInfo>("%WeaponStatInfo");
        var firstItem = GetNode<Control>("%Damage");
        var y = firstItem.GlobalPosition.Y;
        statInfo.GlobalPosition = new Vector2(statInfo.GlobalPosition.X, y);

        statInfo.StatType = statType;
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Weapon is null)
        {
            Hide();
            return;
        }

        var sprite = GetNode<TextureRect>("%WeaponSprite");
        sprite.Texture = Weapon.Config.Sprite;
        var name = GetNode<Label>("%WeaponName");
        name.Text = Weapon.Config.Name;
        var level = GetNode<Label>("%WeaponLevel");
        level.Text = $"Lv. {Weapon.State.Level.CurrentValue}";
        var desc = GetNode<RichTextLabel>("%WeaponDescription");
        desc.Text = Weapon.Config.Description;

        // Factions
        var weaponFaction = Weapon.Config.Faction;
        var max = 3;
        var i = 0;
        foreach (var f in FactionUtil.GetFactionTypes())
        {
            if (i >= max)
            {
                break;
            }

            if (weaponFaction.HasFlag(f))
            {
                var label = GetNode<FacionLabel>($"%FactionLabel{i}");
                label.Faction = f;
                i++;
            }
        }

        for (var j = i; j < max; j++)
        {
            var label = GetNode<FacionLabel>($"%FactionLabel{j}");
            label.Faction = 0u;
        }

        // Stats
        {
            var damage = GetNode<WeaponStatLabel>("%Damage");
            damage.DefaultValue = Weapon.State.Damage.DefaultValue;
            damage.CurrentValue = Weapon.State.Damage.CurrentValue;

            var attackSpeed = GetNode<WeaponStatLabel>("%AttackSpeed");
            {
                // ToDo: 内部化する
                var baseCooldown = Weapon.State.AttackSpeed.DefaultValue;
                var baseAnimation = Weapon.AnimationTime;
                var baseRate = Weapon.State.AttackSpeed.DefaultRate;
                var currentRate = Weapon.State.AttackSpeed.Rate;
                var defaultAttackSpeed = (baseCooldown + baseAnimation) / baseRate;
                var currentAttackSpeed = (baseCooldown + baseAnimation) / currentRate;
                attackSpeed.DefaultValue = (uint)defaultAttackSpeed;
                attackSpeed.CurrentValue = (uint)currentAttackSpeed;
            }

            var knockback = GetNode<WeaponStatLabel>("%Knockback");
            knockback.DefaultValue = Weapon.State.Knockback.DefaultValue;
            knockback.CurrentValue = Weapon.State.Knockback.CurrentValue;

            var lifesteal = GetNode<WeaponStatLabel>("%Lifesteal");
            lifesteal.DefaultValue = (uint)(Weapon.State.LifestealRate.DefaultValue * 100f);
            lifesteal.CurrentValue = (uint)(Weapon.State.LifestealRate.CurrentValue * 100f);
        }

        GetNode<Control>("PanelContainer").ResetSize();
        Show();
    }
}