using System.Collections.Generic;
using Godot;

namespace fms;

public partial class GoldNuggetShop : Node
{
    // GoldNugget の 1金変換レート, 1.2 倍の等比数列
    // 参考: https://scrapbox.io/FUMOSurvivor/ゴールドナゲットテーブル
    private static readonly List<uint> NuggetTable =
    [
        50, 60, 72, 86, 103, 124, 149, 179, 215,
        258, 310, 372, 446, 535, 642, 770, 924,
        1109, 1331, 1597, 1916, 2299, 2759, 3311, 3973,
        4768, 5722, 6866, 8239, 9887, 11864, 14237, 17084
    ];

    public int ShopLevel { get; private set; }

    // 換金する, 成功したら true
    // 換金時に内部の ShopLevel が1増加して次の換金レートが高くなっていく
    public uint ExchangeGoldNugget(uint amount)
    {
        var exchangeAmount = GetNextGoldNuggetAmount();
        if (amount < exchangeAmount)
        {
            return 0;
        }

        ShopLevel++;
        return exchangeAmount;
    }

    public static uint GetGoldNuggetAmount(int idx)
    {
        if (idx < 0 || idx >= NuggetTable.Count)
        {
            return NuggetTable[^1];
        }

        return NuggetTable[idx];
    }

    // 次の換金に必要な GoldNugget の量
    public uint GetNextGoldNuggetAmount()
    {
        // 事前定義したものテーブルを超えたら最後の値を返し続ける
        return ShopLevel >= NuggetTable.Count ? NuggetTable[^1] : NuggetTable[ShopLevel];
    }
}