using System;
using R3;

namespace fms;

public sealed class ShopState : IDisposable
{
    private const int _MAX_TIER = 3;
    private readonly ReactiveProperty<int> _tierRp = new(1);

    public ReadOnlyReactiveProperty<int> Tier => _tierRp;

    public required ShopConfig Config { get; init; }

    public void AddTier()
    {
        if (_tierRp.Value < _MAX_TIER)
        {
            _tierRp.Value++;
        }
    }

    /// <summary>
    ///     Minion をショップから購入
    /// </summary>
    /// <param name="minionData"></param>
    public void BuyItem(MinionCoreData minionData)
    {
        // プレイヤーのお金を減らす
        Main.PlayerState.AddEffect(new AddMoneyEffect { Value = -minionData.Price });
        Main.PlayerState.SolveEffect();

        // すでに Minion を所持している場合
        if (Main.PlayerInventory.TryGetEquippedMinion(minionData, out var minion))
        {
            // Minion をレベルアップ
            minion!.AddEffect(new AddLevelEffect { Value = 1 });
            minion.SolveEffect();
            return;
        }

        Main.PlayerInventory.AddMinion(minionData);

        // Note: 現在購入後デフォで装備にしています
        // 満タンの場合 とか ドラッグで購入とかで色々変わります
        Main.PlayerInventory.EquipMinion(minionData);
    }

    /// <summary>
    ///     Minion をショップに売却
    /// </summary>
    /// <param name="minionData"></param>
    public void SellItem(MinionCoreData minionData)
    {
        if (Main.PlayerInventory.RemoveMinion(minionData))
        {
            // プレイヤーのお金を増やす
            // TODO: 売却価格を売値と同じにしています
            Main.PlayerState.AddEffect(new AddMoneyEffect { Value = minionData.Price });
            Main.PlayerState.SolveEffect();
        }
    }

    void IDisposable.Dispose()
    {
        _tierRp.Dispose();
    }
}