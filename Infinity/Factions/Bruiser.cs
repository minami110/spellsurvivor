namespace fms.Faction;

/// <summary>
///     Lv2: Player の最大体力を 50 上げる (100 => 150)
///     Lv4: Player の最大体力 150 上げる (100 => 250)
///     Lv6: Player の最大体力 450 上げる (100 => 500)
/// </summary>
public sealed class Bruiser : FactionBase
{
    private protected override void OnLevelConfirmed(int level)
    {
        var value = level switch
        {
            >= 6 => 450,
            >= 4 => 150,
            >= 2 => 50,
            _ => 0
        };

        if (value == 0)
        {
            return;
        }

        CreateEffect(value);
    }

    private void CreateEffect(int value)
    {
        // エフェクトを新規作成して, 抜港済みエフェクトとしてマークしておく
        var effect = new AddMaxHealthEffect { Value = value };
        OnEffectPublished(effect);

        // PlayerState に Effect を追加して, 解決処理を依頼する
        Main.PlayerState.AddEffect(effect);
    }
}