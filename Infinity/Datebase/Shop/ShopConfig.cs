using Godot;

namespace fms;

[GlobalClass]
public partial class ShopConfig : FmsResource
{
    [Export]
    public int RerollCost { get; private set; } = 1;

    [Export]
    public int UpgradeCost { get; private set; } = 1;

    [Export]
    public int AddSlotCost { get; private set; } = 1;

    [Export(PropertyHint.Dir)]
    public string ShopItemRootDir { get; private set; } = string.Empty;

    /// <summary>
    ///     Shop Level  別 Tier 別の 排出率
    /// </summary>
    public float[] Odds { get; private set; } =
    {
        // Level 1: 100% 0% 0% 0% 0%
        1f, 0f, 0f, 0f, 0f,
        // Level 2: 75% 25% 0% 0% 0%
        0.75f, 1f, 0f, 0f, 0f,
        // Level 3: 60% 30% 10% 0% 0%
        0.6f, 0.75f, 1f, 0f, 0f,
        // Level 4: 45% 35% 18% 2% 0%
        0.45f, 0.636f, 0.9f, 1f, 0f,
        // Level 5: 30% 40% 25% 5% 0%
        0.3f, 0.571f, 0.833f, 1f, 0f,
        // Level 6: 20% 33% 36% 10% 1%
        0.2f, 0.413f, 0.766f, 0.91f, 1f,
        // Level 7: 15% 30% 32% 20% 3%
        0.15f, 0.353f, 0.582f, 0.869f, 1f,
        // Level 8: 10% 20% 25% 35% 10%
        0.1f, 0.222f, 0.357f, 0.778f, 1f
    };
}