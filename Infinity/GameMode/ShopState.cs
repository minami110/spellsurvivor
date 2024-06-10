using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using R3;

namespace fms;

public partial class ShopState : Node
{
    [Export]
    public ShopConfig Config { get; private set; } = null!;

    private readonly List<Minion> _inStoreMinions = new();
    private readonly Subject<Unit> _inStoreMinionsUpdatedSubject = new();

    private readonly ReactiveProperty<int> _levelRp = new(1);

    private readonly Dictionary<int, List<Minion>> _runtimeMinionPool = new();

    private int _itemSlotCount;

    /// <summary>
    /// </summary>
    public ReadOnlyReactiveProperty<int> Level => _levelRp;

    /// <summary>
    /// </summary>
    public Observable<Unit> InStoreMinionsUpdated => _inStoreMinionsUpdatedSubject;

    /// <summary>
    /// </summary>
    public IReadOnlyList<Minion> InStoreMinions => _inStoreMinions;

    public bool IsLocked { get; set; }

    public ShopState(ShopConfig config) : this()
    {
        Config = config;
    }

    private ShopState()
    {
        // Parameterless constructor for Godot Editor
    }

    public override void _EnterTree()
    {
        // Set Name (for debugging)
        Name = nameof(ShopState);

        // Construct Runtime Minion Pool
        _runtimeMinionPool.Clear();
        foreach (var minionCoreData in Config.DefaultMinionPool)
        {
            if (!_runtimeMinionPool.TryGetValue(minionCoreData.Tier, out var list))
            {
                list = new List<Minion>();
                _runtimeMinionPool[minionCoreData.Tier] = list;
            }

            list.Add(new Minion(minionCoreData));
        }

        // Default 
        _levelRp.Value = 1;
        _itemSlotCount = 3;
    }

    public override void _ExitTree()
    {
        _levelRp.Dispose();
    }

    public void AddItemSlot()
    {
        if (_itemSlotCount < Constant.SHOP_MAX_ITEM_SLOT)
        {
            _itemSlotCount++;
        }
    }

    /// <summary>
    ///     Minion をショップから購入
    /// </summary>
    /// <param name="minion"></param>
    public void BuyItem(Minion minion)
    {
        if (!_inStoreMinions.Contains(minion))
        {
            throw new InvalidProgramException("購入対象の Minion が現在販売されていません");
        }

        // ショップから排除する
        _inStoreMinions.Remove(minion);
        _inStoreMinionsUpdatedSubject.OnNext(Unit.Default);

        // ToDo: 外部化していいかも
        // プレイヤーのお金を減らす
        Main.PlayerState.AddEffect(new MoneyEffect { Value = -minion.Price });
        Main.PlayerState.SolveEffect();

        // Player がすでに Minion を所持していたらレベルを上げる
        var player = this.GetPlayerNode();
        var minions = player.FindChildren("*", nameof(Minion), false, false);
        if (minions.Any(m => m == minion))
        {
            minion.SetLevel(minion.Level.CurrentValue + 1);
            return;
        }

        // Playerは まだ所有していないので子にする
        player.AddChild(minion);

        // ToDo: 現在購入後デフォで装備にしています
        minion.AddWeapon();
    }

    public void RefreshInStoreMinions()
    {
        // ショップがロックされているときは Refresh しない
        if (IsLocked)
        {
            GD.Print("[ShopState] Shop is locked. Refresh skipped.");
            return;
        }

        _inStoreMinions.Clear();

        // スロットカウントの数だけ Pool から Minion を取り出す
        var tryCount = _itemSlotCount - _inStoreMinions.Count;
        if (tryCount <= 0)
        {
            GD.Print("[ShopState] No need to refresh. Refresh skipped.");
            return;
        }

        for (var i = 0; i < tryCount; i++)
        {
            // ティアを決定する
            var targetTier = 1;
            for (; targetTier <= Constant.MINION_MAX_TIER; targetTier++)
            {
                var odds = Config.Odds[(Level.CurrentValue - 1) * Constant.MINION_MAX_TIER + targetTier - 1];
                if (odds < GD.Randf())
                {
                    continue;
                }

                break;
            }

            // ティアが決定したので Minion を選択する
            Minion? minion = null;
            if (_runtimeMinionPool.TryGetValue(targetTier, out var minions))
            {
                // List から ランダムに Minion を取り出す, 最大レベルの場合はスキップする
                var indexes = RangeShuffled(new Random(), minions.Count);
                foreach (var index in indexes)
                {
                    var m = minions[index];
                    if (m.IsMaxLevel)
                    {
                        continue;
                    }

                    minion = m;
                    break;
                }
            }

            if (minion == null)
            {
                throw new NotImplementedException("有効な Minion が見つかりませんでした");
            }

            _inStoreMinions.Add(minion);
        }

        _inStoreMinionsUpdatedSubject.OnNext(Unit.Default);
    }

    /// <summary>
    ///     Minion をショップに売却
    /// </summary>
    /// <param name="minion"></param>
    public void SellItem(Minion minion)
    {
        var player = this.GetPlayerNode();
        var minions = player.FindChildren("*", nameof(Minion), false, false);
        if (minions.Any(m => m == minion))
        {
            // ミニオンをプレイヤーの手持ちから取り除く
            player.RemoveChild(minion);
            // 次も Ready してほしいのでフラグを立てておく
            minion.RequestReady();

            // プレイヤーのお金を増やす
            // TODO: 売却価格を売値と同じにしています
            Main.PlayerState.AddEffect(new MoneyEffect { Value = minion.Price });
            Main.PlayerState.SolveEffect();
        }
    }

    public void UpgradeShopLevel()
    {
        // ToDo: Level Up の値段を設定していない
        if (_levelRp.Value < Constant.SHOP_MAX_LEVEL)
        {
            _levelRp.Value++;
        }
    }

    private static int[] RangeShuffled(Random rng, int count)
    {
        var indexes = Enumerable.Range(0, count).ToArray();
        for (var j = indexes.Length - 1; j > 0; j--)
        {
            var k = rng.Next(j + 1);
            (indexes[j], indexes[k]) = (indexes[k], indexes[j]);
        }

        return indexes;
    }
}