using System;
using System.Collections.Generic;
using Godot;

namespace fms.Faction;

/// <summary>
/// Faction の種類
/// Note: Flag を使っているため 32 コまでしか定義できないです
/// </summary>
[Flags]
public enum FactionType
{
    Bruiser = 1 << 0,
    Duelist = 1 << 1,
    Trickshot = 1 << 2,
    Invoker = 1 << 3,
    Healer = 1 << 4,
    Scrap = 1 << 5,
    Licium = 1 << 6,
    Incandescent = 1 << 7

    // Note: ↓ に新しい Faction を追加していく
}

public static class FactionUtil
{
    private static readonly Dictionary<FactionType, FactionBase> _defaultMap = new();

    public static FactionBase CreateFaction(FactionType faction)
    {
        return faction switch
        {
            FactionType.Bruiser => new Bruiser(),
            FactionType.Duelist => new Duelist(),
            FactionType.Trickshot => new Trickshot(),
            FactionType.Invoker => new Invoker(),
            FactionType.Healer => new Healer(),
            FactionType.Scrap => new Scrap(),
            FactionType.Licium => new Licium(),
            FactionType.Incandescent => new Incandescent(),

            // Note: ↓ に新しい Faction を追加していく

            _ => throw new ArgumentException($"Unsupported faction type: {faction}", nameof(faction))
        };
    }

    public static FactionType GetFactionType(this FactionBase faction)
    {
        var type = faction.GetType().Name;
        return Enum.Parse<FactionType>(type);
    }

    public static IEnumerable<FactionType> GetFactionTypes()
    {
        return Enum.GetValues<FactionType>();
    }

    public static IDictionary<uint, string> GetLevelDescriptions(this FactionType faction)
    {
        var defaultFaction = GetDefaultFaction(faction);
        return defaultFaction.LevelDescriptions;
    }

    public static string GetMajorDescription(this FactionType faction)
    {
        var defaultFaction = GetDefaultFaction(faction);
        return defaultFaction.MainDescription;
    }

    /// <summary>
    /// Faction のアイコンを取得する
    /// </summary>
    /// <param name="faction"></param>
    /// <returns></returns>
    public static Texture2D? GetTextureResouce(this FactionType faction)
    {
        var path = $"res://base/textures/factions/{faction.ToString().ToLower()}.png";

        // Is exist?
        if (!ResourceLoader.Exists(path))
        {
            GD.PrintErr($"Failed to load texture: {path}");
            return null;
        }

        return ResourceLoader.Load<Texture2D>(path);
    }

    private static FactionBase GetDefaultFaction(FactionType faction)
    {
        if (!_defaultMap.TryGetValue(faction, out var factionBase))
        {
            factionBase = CreateFaction(faction);
            _defaultMap[faction] = factionBase;
        }

        return factionBase;
    }
}