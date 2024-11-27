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

    private readonly List<WeaponCard> _inStoreWeaponCards = new();
    private readonly Subject<Unit> _inStoreWeaponCardsUpdatedSubject = new();

    private readonly ReactiveProperty<int> _levelRp = new(1);

    private readonly Dictionary<int, List<WeaponCard>> _runtimeMinionPool = new();

    private int _itemSlotCount;

    /// <summary>
    /// 現在のショップレベル, 排出するアイテムの ティア に影響する
    /// </summary>
    public ReadOnlyReactiveProperty<int> Level => _levelRp;

    /// <summary>
    /// ショップで販売している WeaponCard が更新されたときに通知
    /// </summary>
    public Observable<Unit> InStoreWeaponCardsUpdated => _inStoreWeaponCardsUpdatedSubject;

    /// <summary>
    /// 現在ショップで販売している WeaponCard のリスト
    /// </summary>
    public IReadOnlyList<WeaponCard> InStoreWeaponCards => _inStoreWeaponCards;

    /// <summary>
    /// 品揃えの更新が現在ロックされているかどうか
    /// </summary>
    public bool Locked { get; set; }

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
    /// WeaponCard をショップから購入
    /// </summary>
    /// <param name="weaponCard"></param>
    public void BuyWeaponCard(IEntity entity, WeaponCard weaponCard)
    {
        if (!_inStoreWeaponCards.Contains(weaponCard))
        {
            throw new InvalidProgramException("購入対象の WeaponCard が現在販売されていません");
        }

        // ショップから排除する
        _inStoreWeaponCards.Remove(weaponCard);
        _inStoreWeaponCardsUpdatedSubject.OnNext(Unit.Default);

        weaponCard.OnBuy(entity);
    }


    /// <summary>
    /// 現在のショップのアイテムをリフレッシュする
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void RefreshWeaponCards()
    {
        // ショップがロックされているときは Refresh しない
        if (Locked)
        {
            this.DebugLog("Shop is locked. Refresh skipped.");
            return;
        }

        _inStoreWeaponCards.Clear();

        // スロットカウントの数だけ Pool から Minion を取り出す
        var tryCount = _itemSlotCount - _inStoreWeaponCards.Count;
        if (tryCount <= 0)
        {
            this.DebugLog("No need to refresh. Refresh skipped.");
            return;
        }

        for (var i = 0; i < tryCount; i++)
        {
            // はじめにティアのガチャを行う
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
    }

    /// <summary>
    /// WeaponCard をショップに売却
    /// </summary>
    /// <param name="weaponCard"></param>
    public void SellWeaponCard(IEntity entity, WeaponCard weaponCard)
    {
        weaponCard.OnSell(entity);
    }

    public void UpgradeShopLevel()
    {
        // ToDo: Level Up の値段を設定していない
        if (_levelRp.Value < Constant.SHOP_MAX_LEVEL)
        {
            _levelRp.Value++;
        }
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
                    if (!_runtimeMinionPool.TryGetValue(minionCoreData.Tier, out var list))
                    {
                        list = new List<WeaponCard>();
                        _runtimeMinionPool[minionCoreData.Tier] = list;
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