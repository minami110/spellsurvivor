using Godot;
using R3;

namespace fms.HUD;

public partial class WeaponStatLabel : HBoxContainer
{
    [Export]
    private string _toastKey = "Shop/OwnedWeapon/Stat";

    [Export]
    public string StatType
    {
        get;
        set
        {
            field = value;
            UpdateUi();
        }
    } = string.Empty;

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
    private readonly Subject<string> _requestShowInfo = new();
    public Observable<string> RequestShowInfo => _requestShowInfo;
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
                if (StatType == string.Empty)
                {
                    return;
                }

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

        if (StatType == string.Empty)
        {
            Hide();
            return;
        }

        var sprite = GetNode<TextureRect>("Sprite");
        sprite.Texture = GD.Load<Texture2D>($"res://Common/Resources/Textures/Stats/{StatType}.png");

        var valueLabel = GetNode<Label>("Value");
        if (DefaultValue == CurrentValue)
        {
            valueLabel.Text = DefaultValue.ToString();
            valueLabel.Modulate = Colors.White;
        }
        else
        {
            var isSign = CurrentValue > DefaultValue;
            if (isSign)
            {
                valueLabel.Text = $"{CurrentValue} (+ {CurrentValue - DefaultValue})";
                valueLabel.Modulate = Colors.Green;
            }
            else
            {
                valueLabel.Text = $"{CurrentValue} (- {DefaultValue - CurrentValue})";
                valueLabel.Modulate = Colors.Red;
            }
        }

        Show();
    }
}