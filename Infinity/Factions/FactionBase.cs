using System;
using fms.Effects;

namespace fms.Faction;

public abstract class FactionBase
{
    public int Level { get; private set; }

    public void AddLevel(int amount = 1)
    {
        Level += amount;
    }


    public virtual void OnBattleStarted()
    {
    }

    public virtual void OnEquipped()
    {
    }
}

/// <summary>
///     Lv2: Player の最大体力を 50 上げる (100 => 150)
///     Lv4: Player の最大体力 150 上げる (100 => 250)
///     Lv6: Player の最大体力 450 上げる (100 => 500)
/// </summary>
public sealed class Bruiser : FactionBase
{
    public override void OnEquipped()
    {
        var playerState = Main.PlayerState;
        switch (Level)
        {
            case >= 6:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 400 });
                break;
            case >= 4:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 150 });
                break;
            case >= 2:
                playerState.AddEffect(new AddMaxHealthEffect { Value = 50 });
                break;
        }
    }
}

/// <summary>
///     Lv2: デュエリスト を持つミニオンのクールダウンを 10%減少させる
///     Lv4: デュエリスト を持つミニオンのクールダウンを 20%減少させる
///     Lv6: 自分が持っているすべてのミニオンのクールダウンを 20%減少させる
/// </summary>
public sealed class Duelist : FactionBase
{
    public EffectBase[] GetEffects()
    {
        return Level switch
        {
            >= 6 => new EffectBase[] { new DuelistOnlyMinionCoolDownReduce { Value = 10 } },
            >= 4 => new EffectBase[] { new DuelistOnlyMinionCoolDownReduce { Value = 20 } },
            >= 2 => new EffectBase[] { new AllMinionCoolDownReduce { Value = 20 } },
            _ => Array.Empty<EffectBase>()
        };
    }
}

/// <summary>
///     Lv2: 10秒毎 に一時的なシールド "10" を得る
///     Lv4: 10秒毎 に一時的なシールド "25" を得る
///     Lv6: 10秒毎 に一時的なシールド "25 + Player" の最大体力の 2% を得る
/// </summary>
public sealed class Knight : FactionBase
{
    public EffectBase[] GetEffects()
    {
        return Level switch
        {
            >= 6 => new EffectBase[] { new AddMaxHealthEffect { Value = 400 } },
            >= 4 => new EffectBase[] { new AddMaxHealthEffect { Value = 150 } },
            >= 2 => new EffectBase[] { new AddMaxHealthEffect { Value = 50 } },
            _ => Array.Empty<EffectBase>()
        };
    }
}

/// <summary>
///     トリックショットを持つミニオンの通常攻撃が敵にあたった時、範囲内の最も近い敵に向かって反射する
///     Lv2: 跳ね返り回数 1、 跳ね返った攻撃倍率 40 %
///     Lv4: 跳ね返り回数 2、 跳ね返った攻撃倍率 60 %
/// </summary>
public sealed class Trickshot : FactionBase
{
}