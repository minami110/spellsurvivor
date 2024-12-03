using Godot;
using R3;

namespace fms.HUD;

public partial class OwnedWeaponContainer : Control
{
    [Export]
    private int _maxSlotCount = 6;


    public override void _Ready()
    {
        // プレイヤーの子階層が変わるたびに、OwnedWeaponを更新する
        // ToDo: 重い可能性あり
        var player = this.GetPlayerNode();
        player.ChildOrderChangedAsObservable()
            .Subscribe(_ =>
            {
                var playerNode = this.GetPlayerNode();

                var slotIndex = 0;
                foreach (var n in playerNode.GetChildren())
                {
                    if (n is not WeaponBase weapon)
                    {
                        continue;
                    }

                    var ownedWeapon = GetNode<OwnedWeapon>($"%OwnedWeapon{slotIndex}");
                    ownedWeapon.Weapon = weapon;

                    slotIndex++;
                }

                for (var i = slotIndex; i < _maxSlotCount; i++)
                {
                    var ownedWeapon = GetNode<OwnedWeapon>($"%OwnedWeapon{i}");
                    ownedWeapon.Weapon = null;
                }
            })
            .AddTo(this);

        // すべてのスロットからの Toast 表示通知を受け取る
        for (var i = 0; i < _maxSlotCount; i++)
        {
            var n = GetNode<OwnedWeapon>($"%OwnedWeapon{i}");
            n.RequestShowInfo
                .Subscribe(ShowToast)
                .AddTo(this);
            n.RequestHideInfo
                .Subscribe(_ => HideToast())
                .AddTo(this);
        }
    }

    private void HideToast()
    {
        var info = GetNode<OwnedWeaponInfo>("%OwnedWeaponInfo");
        info.Weapon = null;
    }

    private void ShowToast(WeaponBase weapon)
    {
        var info = GetNode<OwnedWeaponInfo>("%OwnedWeaponInfo");
        info.Weapon = weapon;
    }
}