using System;
using Godot;
using R3;

namespace fms;

/// <summary>
///     Player が所有済みの Minion の情報
/// </summary>
public sealed class MinionInInventory : IDisposable
{
    private const int _MIN_LEVEL = 1;
    private const int _MAX_LEVEL = 5;

    private readonly ReactiveProperty<int> _levelRp = new(1);

    /// <summary>
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<int> Level => _levelRp;

    public bool IsMaxLevel => _levelRp.Value >= _MAX_LEVEL;

    public bool IsLocked { get; private set; }

    public int Price { get; }

    public Texture2D Sprite { get; }

    public string Name { get; }

    public string Description { get; }

    public PackedScene WeaponPackedScene { get; }

    public MinionInInventory(MinionCoreData minionCoreData)
    {
        Id = minionCoreData.Id;
        Price = minionCoreData.Price;
        Sprite = minionCoreData.Sprite;
        Name = minionCoreData.Name;
        Description = minionCoreData.Description;
        WeaponPackedScene = minionCoreData.WeaponPackedScene;
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void SetLevel(int level)
    {
        _levelRp.Value = Math.Clamp(level, _MIN_LEVEL, _MAX_LEVEL);
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    public void Dispose()
    {
        _levelRp.Dispose();
    }
}