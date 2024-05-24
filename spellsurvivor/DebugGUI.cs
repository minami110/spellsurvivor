using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godot;

namespace fms;

public partial class DebugGUI : CanvasLayer
{
    private static DebugGUI? _instance;
    private readonly Dictionary<string, string> _dict = new();
    private readonly StringBuilder _sb = new();
    private Label _label = null!;

    public override void _Ready()
    {
        if (_instance is not null)
        {
            throw new ApplicationException("DebugGUI instance already exists");
        }

        _instance = this;

        // Cache node
        _label = GetNode<Label>("DictionaryLabel");

        // First value
        _dict["FPS"] = "0";
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void CommitText(string key, string value)
    {
        if (_instance is not null)
        {
            _instance._dict[key] = value;
        }
    }

    public static bool RemoveKey(string key)
    {
        if (_instance is null)
        {
            return false;
        }

        var removeKeys = new List<string>();
        foreach (var (k, _) in _instance._dict)
            if (k.Contains(key))
            {
                removeKeys.Add(k);
            }

        foreach (var k in removeKeys) _instance._dict.Remove(k);

        return true;
    }

    public override void _Process(double delta)
    {
        // Update FPS
        var fps = Engine.GetFramesPerSecond();
        _dict["FPS"] = fps.ToString(CultureInfo.InvariantCulture);

        // Update Label
        foreach (var (key, value) in _dict)
        {
            _sb.Append($"{key}: {value}\n");
        }

        _label.Text = _sb.ToString();
        _sb.Clear();
    }
}