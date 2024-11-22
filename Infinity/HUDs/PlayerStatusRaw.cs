using Godot;

namespace fms;

/// <summary>
/// Stats 画面の各行 の UI
/// </summary>
[Tool]
public partial class PlayerStatusRaw : HBoxContainer
{
    /// <summary>
    /// Stats のアイコン
    /// </summary>
    [Export]
    public Texture2D? Icon
    {
        get;
        set
        {
            field = value;
            UpdateIcon();
        }
    } = null;

    /// <summary>
    /// Stats のラベル (ToDo: ローカライズ対応)
    /// </summary>
    [Export]
    public string Label
    {
        get;
        set
        {
            field = value;
            UpdateLabel();
        }
    } = string.Empty;

    /// <summary>
    /// Stats のデフォルト値
    /// </summary>
    [Export]
    public string DefaultValue
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = string.Empty;

    /// <summary>
    /// Stats の現在値
    /// </summary>
    [Export]
    public string Value
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = string.Empty;

    [Export]
    public string Prefix
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = string.Empty;

    [Export]
    public string Suffix
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = string.Empty;

    [Export]
    public Color FontColor
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = new(1, 1, 1);

    /// <summary>
    /// Stats の現在値がデフォルト値と異なる場合のフォントカラー
    /// </summary>
    [Export]
    public Color NonDefaultFontColor
    {
        get;
        set
        {
            field = value;
            UpdateValue();
        }
    } = new(0, 1, 0);

    private Label ValueLabel => GetNode<Label>("Value");

    private void UpdateIcon()
    {
        GetNode<TextureRect>("Icon").Texture = Icon;
    }

    private void UpdateLabel()
    {
        GetNode<Label>("Label").Text = Label;
    }

    private void UpdateValue()
    {
        ValueLabel.Text = $"{Prefix} {Value} {Suffix}";
        ValueLabel.Modulate = DefaultValue != Value ? NonDefaultFontColor : FontColor;
    }
}