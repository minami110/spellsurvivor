using Godot;

namespace fms;

public partial class SettingTab : TabContainer
{
    [Export]
    private Slider _masterVolumeSlider = null!;

    [Export]
    private MenuButton _languageMenuButton = null!;

    public override void _Ready()
    {
        var config = GameConfig.Singleton;

        // Master Volume Slide initialization and bindings
        _masterVolumeSlider.MinValue = 0;
        _masterVolumeSlider.MaxValue = 1;
        _masterVolumeSlider.Step = 0.05f;
        _masterVolumeSlider.Value = config.AudioMasterVolume.Value;
        _masterVolumeSlider.ValueChanged += value => { config.AudioMasterVolume.Value = (float)value; };

        // Language PopupMenu initialization and bindings
        var popup = _languageMenuButton.GetPopup();

        var locales = TranslationServer.GetLoadedLocales();
        foreach (var locale in locales)
        {
            switch (locale)
            {
                case "en":
                    popup.AddItem("English");
                    break;
                case "ja":
                    popup.AddItem("日本語");
                    break;
            }
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


    public override void _ExitTree()
    {
        // Save config
        var config = GameConfig.Singleton;
        config.SaveConfig();
    }

    private void UpdateLocale(string locale)
    {
        switch (locale)
        {
            case "en":
                _languageMenuButton.Text = "English";
                TranslationServer.SetLocale(locale);
                break;
            case "ja":
                _languageMenuButton.Text = "日本語";
                TranslationServer.SetLocale(locale);
                break;
            default:
                _languageMenuButton.Text = "English";
                TranslationServer.SetLocale("en");
                break;
        }
    }
}