using System;
using System.Collections.Generic;

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
    Scrap = 1 << 5

    // Note: ↓ に新しい Faction を追加していく
}

public static class FactionUtil
{
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
            // Note: ↓ に新しい Faction を追加していく

            _ => throw new ArgumentException($"Unsupported faction type: {faction}", nameof(faction))
        };
    }

    public static IEnumerable<FactionType> GetFactionTypes()
    {
        return Enum.GetValues<FactionType>();
    }
}