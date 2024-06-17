using System.Collections.Generic;
using fms.Weapon;
using Godot;
using R3;

namespace fms;

internal partial class BattleResultHudController : Node
{
    [Export]
    private Button _acceptButton = null!;

    [Export]
    private Label _titleLabel = null!;

    [Export]
    private Label _rewardLabel = null!;

    public override void _Ready()
    {
        GetNode<Control>("%RootPanel").Hide();

        var ws = Main.WaveState;
        var d1 = ws.Phase.Subscribe(this, (p, self) =>
        {
            if (p == WavePhase.Battleresult)
            {
                var playerState = (PlayerState)self.GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayerState);
                var reward = Main.WaveState.CurrentWaveConfig.Reward;
                var playerMoney = playerState.Money.CurrentValue;

                self._titleLabel.Text = $"Wave {Main.WaveState.Wave.CurrentValue} Result";
                self._rewardLabel.Text = $"Money: ${playerMoney + reward} (+${reward})";
                self.GetNode<Control>("%RootPanel").Show();

                // 武器の統計情報を表示する
                var results = new Dictionary<ulong, float>();
                var vbox = self.GetNode<VBoxContainer>("%VBoxWeaponStats");

                var stats = StaticsManager.DamageInfoTable;

                // 現在 Player が装備している武器を取得する
                var player = self.GetPlayerNode();
                foreach (var n in player.GetChildren())
                {
                    if (n is WeaponBase weapon)
                    {
                        var uid = weapon.GetInstanceId();
                        if (stats.TryGetValue(uid, out var info))
                        {
                            foreach (var i in info)
                            {
                                if (results.TryGetValue(uid, out var damage))
                                {
                                    results[uid] += i.Amount;
                                }
                                else
                                {
                                    results[uid] = i.Amount;
                                }
                            }
                        }
                    }
                }

                foreach (var n in player.GetChildren())
                {
                    if (n is WeaponBase weapon)
                    {
                        var uid = n.GetInstanceId();
                        if (results.TryGetValue(uid, out var damage))
                        {
                            var label = new Label();
                            label.HorizontalAlignment = HorizontalAlignment.Center;
                            label.Text = $"{weapon.MinionId}: total {damage} damage";
                            vbox.AddChild(label);
                        }
                    }
                }
            }
            else
            {
                self.GetNode<Control>("%RootPanel").Hide();

                // ToDo: ここで 毎 Wave 統計情報を削除しています
                StaticsManager.ClearDamageInfoTable();

                var vbox = self.GetNode<VBoxContainer>("%VBoxWeaponStats");
                // Clear all children
                foreach (var c in vbox.GetChildren())
                {
                    c.QueueFree();
                }
            }
        });

        var d2 = _acceptButton.PressedAsObservable().Subscribe(_ =>
        {
            Main.WaveState.SendSignal(WaveState.Signal.PlayerAcceptedBattleResult);
        });

        Disposable.Combine(d1, d2).AddTo(this);
    }
}