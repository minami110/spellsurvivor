using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using R3;

namespace fms;

public partial class Shop : Node
{
    [Export]
    public ShopConfig Config { get; private set; } = null!;

    private readonly ReactiveProperty<uint> _cardSlotCountRp = new(3);
    private readonly ReactiveProperty<bool> _cardSlotLockedRp = new(false);

    private readonly List<WeaponCard?> _inStoreWeaponCards = new();
    private readonly Subject<Unit> _inStoreWeaponCardsUpdatedSubject = new();
    private readonly ReactiveProperty<uint> _levelRp = new(1);

    private readonly Dictionary<uint, List<WeaponCard>> _runtimeMinionPool = new();


    /// <summary>
    /// 現在のショップレベル, 排出するアイテムの ティア に影響する
    /// </summary>
    public ReadOnlyReactiveProperty<uint> Level => _levelRp;

    public ReadOnlyReactiveProperty<uint> CardSlotCount => _cardSlotCountRp;

    public ReadOnlyReactiveProperty<bool> CardSlotLocked => _cardSlotLockedRp;

    /// <summary>
    /// ショップで販売している WeaponCard が更新されたときに通知
    /// </summary>
    public Observable<Unit> InStoreWeaponCardsUpdated => _inStoreWeaponCardsUpdatedSubject;

    /// <summary>
    /// 現在ショップで販売している WeaponCard のリスト
    /// </summary>
    public IReadOnlyList<WeaponCard?> InStoreWeaponCards => _inStoreWeaponCards;

    public bool Locked
    {
        set => _cardSlotLockedRp.Value = value;
    }

    public Shop(ShopConfig config) : this()
    {
        Config = config;
    }

    // Parameterless constructor for Godot Editor
    private Shop()
    {
    }

    public override void _EnterTree()
    {
        // Set Name (for debugging)
        Name = nameof(Shop);

        // Construct Runtime Minion Pool
        _runtimeMinionPool.Clear();
        LoadWeaponCards();

        // Default 
        _levelRp.Value = 1;
    }

    public override void _ExitTree()
    {
        _levelRp.Dispose();
        _cardSlotCountRp.Dispose();
    }

    /// <summary>
    /// WeaponCard をショップから購入
    /// </summary>
    public void BuyWeaponCard(IEntity entity, int index)
    {
        var weaponCard = _inStoreWeaponCards[index];
        if (weaponCard is null)
        {
            throw new InvalidOperationException("WeaponCard is sold out");
        }

        weaponCard.OnBuy(entity);

        // WeaponCard の Index を null (売り切れ) にする
        _inStoreWeaponCards[index] = null;
        _inStoreWeaponCardsUpdatedSubject.OnNext(Unit.Default);
    }

    /// <summary>
    /// Weapon Card 販売枠を一つ追加
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool BuyWeaponCardSlot(IEntity entity)
    {
        if (_cardSlotCountRp.Value < Constant.SHOP_MAX_ITEM_SLOT)
        {
            _cardSlotCountRp.Value++;
            entity.State.ReduceMoney(Config.AddSlotCost);

            // 一個枠が増えたので null を販売する
            _inStoreWeaponCards.Add(null);
            _inStoreWeaponCardsUpdatedSubject.OnNext(Unit.Default);

            return true;
        }

        return false;
    }

    public bool RefreshWeaponCardsFromWave()
    {
        // ショップがロックされているときは Refresh しない
        if (_cardSlotLockedRp.Value)
        {
            this.DebugLog("Shop is locked. Refresh skipped.");
            return false;
        }

        _inStoreWeaponCards.Clear();

        // スロットカウントの数だけ Pool から Minion を取り出す
        var tryCount = _cardSlotCountRp.Value - _inStoreWeaponCards.Count;
        if (tryCount <= 0)
        {
            this.DebugLog("No need to refresh. Refresh skipped.");
            return false;
        }

        for (var i = 0; i < tryCount; i++)
        {
            // はじめにティアのガチャを行う
            var targetTier = 1u;
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
            WeaponCard? minion = null;
            if (_runtimeMinionPool.TryGetValue(targetTier, out var minions))
            {
                // List から ランダムに Minion を取り出す, 最大レベルの場合はスキップする
                var indexes = RangeShuffled(new Random(), minions.Count);
                foreach (var index in indexes)
                {
                    var m = minions[index];

                    // ToDo: 最大レベルに達している Minion は排出しない処理

                    minion = m;
                    break;
                }
            }

            if (minion == null)
            {
                throw new NotImplementedException("Pool から排出可能な Minion が見つかりませんでした");
            }

            _inStoreWeaponCards.Add(minion);
        }

        _inStoreWeaponCardsUpdatedSubject.OnNext(Unit.Default);
        return true;
    }

    /// <summary>
    /// 現在のショップのアイテムをリフレッシュする
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void RerollWeaponCards(IEntity entity)
    {
        if (RefreshWeaponCardsFromWave())
        {
            entity.State.ReduceMoney(Config.RerollCost);
        }
    }

    /// <summary>
    /// WeaponCard をショップに売却
    /// </summary>
    /// <param name="weaponCard"></param>
    public void SellWeaponCard(IEntity entity, WeaponCard weaponCard)
    {
        weaponCard.OnSell(entity);
    }

    public bool UpgradeShopLevel(IEntity entity)
    {
        if (_levelRp.Value < Constant.SHOP_MAX_LEVEL)
        {
            _levelRp.Value++;
            entity.State.ReduceMoney(Config.UpgradeCost);
            return true;
        }

        return false;
    }

    private void LoadWeaponCards()
    {
        var searchDir = Config.ShopItemRootDir;

        // Load Minion Data
        this.DebugLog($"Start loading shop items from: {searchDir}");
        using var dir = DirAccess.Open(searchDir);
        if (dir != null)
        {
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            while (fileName != string.Empty)
            {
                // Note: Godot 4.2.2
                // Runtime で XXX.tres.remap となっていることがある (ランダム?)
                // この場合 .remap を抜いたパスを読み込むとちゃんと行ける
                // See https://github.com/godotengine/godot/issues/66014
                if (fileName.EndsWith(".tres") || fileName.EndsWith(".tres.remap"))
                {
                    if (fileName.EndsWith(".remap"))
                    {
                        fileName = fileName.Replace(".remap", string.Empty);
                    }

                    var path = Path.Combine(searchDir, fileName);
                    var minionCoreData = ResourceLoader.Load<MinionCoreData>(path);
                    GD.Print($"  Loaded: {path} => {minionCoreData.Name}");
                    if (!_runtimeMinionPool.TryGetValue((uint)minionCoreData.TierType, out var list))
                    {
                        list = new List<WeaponCard>();
                        _runtimeMinionPool[(uint)minionCoreData.TierType] = list;
                    }

                    // WeaponCard を作成 (ツリーに入れずにメモリで管理しておく)
                    list.Add(new WeaponCard(minionCoreData));
                }

                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory not found: {searchDir}");
        }

        this.DebugLog("Completed!");
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