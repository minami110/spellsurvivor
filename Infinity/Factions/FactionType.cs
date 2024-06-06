using System;

namespace fms.Faction;

/// <summary>
///     Faction の種類
///     Note: Flag を使っているため 32 コまでしか定義できないです
/// </summary>
[Flags]
public enum FactionType
{
    Bruiser = 1 << 0,
    Duelist = 1 << 1,
    Trickshot = 1 << 2,
    Invoker = 1 << 3
}