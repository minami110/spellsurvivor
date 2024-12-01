using Godot;
using R3;

namespace fms.HUD;

/// <summary>
/// FactionInfo の下部に表示される Faction に所属している武器一覧のサムネイル
/// </summary>
public partial class WeaponMiniThumbnail : Control
{
    [Export]
    private string _focusKey = "Shop/OwnedWeapon/Faction/Weapon";

    private readonly Subject<Unit> _requestHideInfo = new();
    private readonly Subject<WeaponCard> _requestShowInfo = new();

    private Color _baseModulate;
    public Observable<WeaponCard> RequestShowInfo => _requestShowInfo;
    public Observable<Unit> RequestHideInfo => _requestHideInfo;

    public WeaponCard? WeaponCard
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
        this.MouseEnteredAsObservable()
            .Subscribe(_ => { Modulate = _baseModulate + new Color(.05f, .05f, .05f); })
            .AddTo(this);

        this.MouseExitedAsObservable()
            .Subscribe(_ => { Modulate = _baseModulate; })
            .AddTo(this);

        this.FocusEnteredAsObservable()
            .Subscribe(_ =>
            {
                if (WeaponCard is null)
                {
                    return;
                }

                ToastManager.Instance.CommitFocusEntered(_focusKey);
                _requestShowInfo.OnNext(WeaponCard);
            })
            .AddTo(this);

        ToastManager.Instance.FocusEntered
            .Subscribe(key =>
            {
                if (!key.StartsWith(_focusKey))
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

        if (WeaponCard == null)
        {
            Hide();
            return;
        }

        // Tier to bg color
        var tier = WeaponCard.Tier;
        var bg = GetNode<ColorRect>("BG");
        bg.Color = tier.ToColor();

        // Update sprite
        var texture = WeaponCard.Sprite;
        var sprite = GetNode<TextureRect>("Sprite");
        sprite.Texture = texture;

        // Is already owned card?
        var isOwned = WeaponCard.IsOwned;
        _baseModulate = isOwned ? Colors.White : new Color(.4f, .4f, .4f);
        Modulate = _baseModulate;

        Show();
    }
}