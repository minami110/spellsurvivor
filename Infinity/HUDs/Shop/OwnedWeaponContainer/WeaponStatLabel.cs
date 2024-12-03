using Godot;
using R3;

namespace fms.HUD;

public partial class WeaponStatLabel : HBoxContainer
{
    [Export]
    private string _toastKey = "Shop/OwnedWeapon/Stat";

    [Export]
    public WeaponStatusType StatType
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    } = WeaponStatusType.Damage;

    [Export]
    private ValueType _valueType = ValueType.Raw;

    [Export]
    public uint DefaultValue
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    [Export]
    public uint CurrentValue
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    }

    private readonly Subject<Unit> _requestHideInfo = new();
    private readonly Subject<WeaponStatusType> _requestShowInfo = new();
    public Observable<WeaponStatusType> RequestShowInfo => _requestShowInfo;
    public Observable<Unit> RequestHideInfo => _requestHideInfo;

    public override void _Ready()
    {
        this.MouseEnteredAsObservable()
            .Subscribe(_ => { Modulate = new Color(1.5f, 1.5f, 1.1f); })
            .AddTo(this);

        this.MouseExitedAsObservable()
            .Subscribe(_ => { Modulate = Colors.White; })
            .AddTo(this);

        this.FocusEnteredAsObservable()
            .Subscribe(_ =>
            {
                ToastManager.Instance.CommitFocusEntered(_toastKey);
                _requestShowInfo.OnNext(StatType);
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

        var sprite = GetNode<TextureRect>("Sprite");
        sprite.Texture = GD.Load<Texture2D>($"res://base/textures/stats/{StatType.ToString().ToSnakeCase()}.png");

        var valueLabel = GetNode<Label>("Value");
        if (DefaultValue == CurrentValue)
        {
            valueLabel.Text = _valueType switch
            {
                ValueType.Raw => DefaultValue.ToString(),
                ValueType.FrameSpeed => $"{DefaultValue / 60f:0.00}",
                _ => valueLabel.Text
            };
            valueLabel.Modulate = Colors.White;
        }
        else
        {
            var isSign = CurrentValue > DefaultValue;
            if (isSign)
            {
                valueLabel.Text = _valueType switch
                {
                    ValueType.Raw => $"{CurrentValue} (+ {CurrentValue - DefaultValue})",
                    ValueType.FrameSpeed => $"{CurrentValue / 60f:0.00} (+ {(CurrentValue - DefaultValue) / 60f:0.00})",
                    _ => valueLabel.Text
                };
                valueLabel.Modulate = Colors.Green;
            }
            else
            {
                valueLabel.Text = _valueType switch
                {
                    ValueType.Raw => $"{CurrentValue} (- {DefaultValue - CurrentValue})",
                    ValueType.FrameSpeed => $"{CurrentValue / 60f:0.00} (- {(DefaultValue - CurrentValue) / 60f:0.00})",
                    _ => valueLabel.Text
                };
                valueLabel.Modulate = Colors.Red;
            }
        }

        Show();
    }

    private enum ValueType
    {
        Raw,
        FrameSpeed
    }
}