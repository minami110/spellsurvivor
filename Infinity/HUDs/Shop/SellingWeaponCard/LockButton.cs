using Godot;
using R3;

namespace fms.HUD;

[Tool]
public partial class LockButton : Button
{
    [Export]
    private bool _locked
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            UpdateUi();

            if (!Engine.IsEditorHint())
            {
                _lockedRp.Value = value;
            }
        }
    } = false;

    private readonly ReactiveProperty<bool> _lockedRp = new(false);

    public ReadOnlyReactiveProperty<bool> Locked => _lockedRp;

    public override void _Ready()
    {
        UpdateUi();

        if (Engine.IsEditorHint())
        {
            return;
        }

        this.PressedAsObservable()
            .Subscribe(_ => { _locked = !_locked; })
            .AddTo(this);

        _lockedRp.AddTo(this);
    }

    private void UpdateUi()
    {
        if (!IsNodeReady())
        {
            return;
        }

        if (_locked)
        {
            GetNode<TextureRect>("UnlockedSprite").Hide();
            GetNode<TextureRect>("LockedSprite").Show();
        }
        else
        {
            GetNode<TextureRect>("UnlockedSprite").Show();
            GetNode<TextureRect>("LockedSprite").Hide();
        }
    }
}