using fms.Faction;
using Godot;
using R3;

namespace fms.HUD;

public partial class FacionLabel : HBoxContainer
{
    [Export]
    private string _toastKey = "Shop/OwnedWeapon/Faction";

    [Export(PropertyHint.Enum)]
    public FactionType Faction
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    private readonly Subject<Unit> _requestHideInfo = new();
    private readonly Subject<FactionType> _requestShowInfo = new();
    public Observable<FactionType> RequestShowInfo => _requestShowInfo;
    public Observable<Unit> RequestHideInfo => _requestHideInfo;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        this.MouseEnteredAsObservable()
            .Subscribe(_ => { Modulate = new Color(1.5f, 1.5f, 1.1f); })
            .AddTo(this);

        this.MouseExitedAsObservable()
            .Subscribe(_ => { Modulate = Colors.White; })
            .AddTo(this);

        this.FocusEnteredAsObservable()
            .Subscribe(_ =>
            {
                if (Faction == 0u)
                {
                    return;
                }

                ToastManager.Instance.CommitFocusEntered(_toastKey);
                _requestShowInfo.OnNext(Faction);
            })
            .AddTo(this);

        ToastManager.Instance.FocusEntered
            .Subscribe(key =>
            {
                if (!key.StartsWith(_toastKey))
                {
                    _requestHideInfo.OnNext(Unit.Default);
                }
            })
            .AddTo(this);

        UpdateUi();
        _requestShowInfo.AddTo(this);
        _requestHideInfo.AddTo(this);
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (Faction == 0u)
        {
            Hide();
            return;
        }

        // ToDo: フラグなので単一でないばあいは怒る

        var sprite = GetNode<TextureRect>("Sprite");
        sprite.Texture = Faction.GetTextureResouce();
        var text = GetNode<Label>("Name");
        text.Text = $"FACTION_{Faction.ToString().ToUpper()}";
        Show();
    }
}