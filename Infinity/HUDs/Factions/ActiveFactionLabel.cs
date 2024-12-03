using System.Linq;
using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class ActiveFactionLabel : PanelContainer
{
    [Export]
    public Color ActiveFontColor { get; set; } = new(0.8f, 0.8f, 0.8f);

    [Export]
    public Color DeactiveFontColor { get; set; } = new(0.3f, 0.3f, 0.3f);

    [Export]
    protected string FocusKey { get; private set; } = "ActiveFactionLabel";

    private readonly Subject<FactionType> _requestShowInfo = new();
    public Observable<FactionType> RequestShowInfo => _requestShowInfo;

    public Observable<Unit> RequestHideInfo =>
        ToastManager.Instance.FocusEntered
            .Where(this, (key, self) => !key.StartsWith(self.FocusKey))
            .AsUnitObservable();

    public FactionBase? Faction
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationMouseEnter)
        {
            UpdateHover(true);
        }
        else if (what == NotificationMouseExit)
        {
            UpdateHover(false);
        }
        else if (what == NotificationFocusEnter)
        {
            ToastManager.Instance.CommitFocusEntered(FocusKey);
            if (Faction is not null)
            {
                _requestShowInfo.OnNext(Faction.GetFactionType());
            }
        }
        else if (what == NotificationReady)
        {
            _requestShowInfo.AddTo(this);
            UpdateUi();
        }
    }

    private void UpdateActivate(bool activate)
    {
        if (activate)
        {
            GetNode<Control>("%Name").SelfModulate = ActiveFontColor;
            GetNode<Control>("%Level").SelfModulate = ActiveFontColor;
            GetNode<Control>("%Name").SelfModulate = ActiveFontColor;
        }
    }

    private void UpdateHover(bool hover)
    {
        GetNode<Control>("%Sprite").Modulate = hover ? new Color(1f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f);
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Faction is null)
        {
            Hide();
            return;
        }

        var type = Faction.GetFactionType();
        var currentLevel = Faction.Level;
        var isActivate = currentLevel >= Faction.LevelDescriptions.First().Key;

        // Sprite
        var sprite = GetNode<TextureRect>("%Sprite");
        sprite.Texture = type.GetTextureResouce();

        // Name
        var name = GetNode<Label>("%Name");
        name.Text = $"FACTION_{type.ToString().ToUpper()}";
        name.SelfModulate = isActivate ? ActiveFontColor : DeactiveFontColor;

        // Level
        var levelLabel = GetNode<Label>("%Level");
        levelLabel.Text = $"{currentLevel}";
        levelLabel.SelfModulate = isActivate ? ActiveFontColor : DeactiveFontColor;

        // Level Description
        var descriptions = Faction.LevelDescriptions;
        var minLevel = 0u;
        var i = 0;
        var maxCount = 4;
        foreach (var (l, d) in descriptions)
        {
            if (i >= maxCount)
            {
                break;
            }

            if (minLevel == 0u)
            {
                minLevel = l;
            }

            var label = GetNode<Label>($"%ActiveLevel{i++}");
            label.Text = l.ToString();
            label.Show();

            label.Modulate = currentLevel >= l ? ActiveFontColor : DeactiveFontColor;
        }

        for (var j = i; j < maxCount; j++)
        {
            GetNode<Label>($"%ActiveLevel{j}").Hide();
        }

        // Show
        ResetSize();
        Show();
    }
}