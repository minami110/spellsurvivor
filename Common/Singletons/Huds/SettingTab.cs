using Godot;
using R3;

namespace fms;

public partial class SettingTab : TabContainer
{
    [Export]
    private Slider _masterVolumeSlider = null!;

    [Export]
    private MenuButton _languageMenuButton = null!;

    [Export]
    private CheckButton _showDamageNumbersCheckButton = null!;

    [Export]
    private CheckButton _debugShowCollisionCheckButton = null!;

    public override void _Ready()
    {
        var config = GameConfig.Singleton;

        // Master Volume Slide initialization and bindings
        {
            _masterVolumeSlider.MinValue = 0;
            _masterVolumeSlider.MaxValue = 1;
            _masterVolumeSlider.Step = 0.05f;
            _masterVolumeSlider.SetValueNoSignal(config.AudioMasterVolume.Value);
            _masterVolumeSlider.ValueChanged += value => { config.AudioMasterVolume.Value = (float)value; };
        }

        // Language PopupMenu initialization and bindings
        {
            var popup = _languageMenuButton.GetPopup();

            var locales = TranslationServer.GetLoadedLocales();
            foreach (var locale in locales)
            {
                popup.AddItem(TranslationServer.GetLocaleName(locale));
            }

            var configLocale = config.Locale.Value;
            UpdateLocale(configLocale);

            popup.IdPressed += x =>
            {
                var locale = TranslationServer.GetLoadedLocales()[x];
                UpdateLocale(locale);
                config.Locale.Value = locale;
            };
        }

        // Show Damage Numbers CheckButton initialization and bindings
        {
            // Fetch current value from config
            _showDamageNumbersCheckButton.SetPressedNoSignal(config.ShowDamageNumbers.Value);
            _showDamageNumbersCheckButton.ToggledAsObservable().Subscribe(x => { config.ShowDamageNumbers.Value = x; })
                .AddTo(this);
        }

        // (Debug) Show Collision CheckButton initialization and bindings
        {
            // Fetch current value from config
            _debugShowCollisionCheckButton.ToggledAsObservable()
                .Subscribe(x => { config.DebugShowCollision.Value = x; })
                .AddTo(this);
            config.DebugShowCollision.Subscribe(x =>
            {
                _debugShowCollisionCheckButton.SetPressedNoSignal(x);
                GetTree().DebugCollisionsHint = x;
            }).AddTo(this);
        }
    }

    public override void _ExitTree()
    {
        // Save config
        var config = GameConfig.Singleton;
        config.SaveConfig();
    }

    private void UpdateLocale(string locale)
    {
        _languageMenuButton.Text = TranslationServer.GetLocaleName(locale);
        TranslationServer.SetLocale(locale);
    }
}