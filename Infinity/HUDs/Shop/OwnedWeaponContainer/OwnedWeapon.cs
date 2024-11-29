using Godot;
using R3;

namespace fms.HUD;

public partial class OwnedWeapon : Control
{
    [Export]
    private string _toastKey = "Shop/OwnedWeapon";

    private readonly Subject<Unit> _requestHideInfo = new();
    private readonly Subject<WeaponBase> _requestShowInfo = new();
    public Observable<WeaponBase> RequestShowInfo => _requestShowInfo;
    public Observable<Unit> RequestHideInfo => _requestHideInfo;

    public WeaponBase? Weapon
    {
        get;
        set
        {
            field = value;
            OnChangedWeapon();
        }
    }

    public override void _Ready()
    {
        this.MouseEnteredAsObservable()
            .Subscribe(_ => { Modulate = new Color(1.1f, 1.1f, 1.1f); })
            .AddTo(this);

        this.MouseExitedAsObservable()
            .Subscribe(_ => { Modulate = Colors.White; })
            .AddTo(this);


        this.FocusEnteredAsObservable()
            .Subscribe(_ =>
            {
                if (Weapon is null)
                {
                    return;
                }

                ToastManager.Instance.CommitFocusEntered(_toastKey);
                _requestShowInfo.OnNext(Weapon);
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

        OnChangedWeapon();

        // Disposable 関連
        _requestShowInfo.AddTo(this);
        _requestHideInfo.AddTo(this);
    }

    private void OnChangedWeapon()
    {
        if (!IsNodeReady())
        {
            return;
        }

        var sprite = GetNode<TextureRect>("Sprite");
        var bg = GetNode<ColorRect>("BG");

        if (Weapon is null)
        {
            sprite.Hide();
            bg.Color = Colors.Black;
        }
        else
        {
            sprite.Show();
            sprite.Texture = Weapon.Config.Sprite;

            var tier = Weapon.Config.Tier;
            bg.Color = tier switch
            {
                TierType.Common => FmsColors.TierCommon,
                TierType.Rare => FmsColors.TierRare,
                TierType.Epic => FmsColors.TierEpic,
                TierType.Legendary => FmsColors.TierLegendary,
                _ => bg.Color
            };
        }
    }
}