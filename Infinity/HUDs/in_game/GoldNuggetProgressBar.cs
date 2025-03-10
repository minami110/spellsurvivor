using Godot;
using R3;

namespace fms;

public partial class GoldNuggetProgressBar : Control
{
    private Label _label = null!;
    private ProgressBar _progressBar = null!;

    public override void _EnterTree()
    {
        _progressBar = GetNode<ProgressBar>("%ProgressBar");
        _label = GetNode<Label>("%Label");

        // Bind player
        var playerState = (EntityState)GetTree().GetFirstNodeInGroup(GroupNames.PlayerState);
        playerState.GoldNugget.ChangedCurrentValue.Subscribe(_on_player_gold_nugget_updated).AddTo(this);
    }

    private void _on_player_gold_nugget_updated(uint amount)
    {
        // 次に必要な GoldNugget の量を取得
        var shopLevel = Main.GoldNuggetShop.ShopLevel;
        var nextGoldNugget = GoldNuggetShop.GetGoldNuggetAmount(shopLevel);

        while (amount >= nextGoldNugget)
        {
            amount -= nextGoldNugget;
            shopLevel++;
            nextGoldNugget = GoldNuggetShop.GetGoldNuggetAmount(shopLevel);
        }

        _progressBar.MaxValue = nextGoldNugget;
        _progressBar.Value = amount;
        _label.Text = $"{amount}/{nextGoldNugget}";
    }
}