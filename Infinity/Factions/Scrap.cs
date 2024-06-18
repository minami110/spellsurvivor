using System;
using System.Collections.Generic;
using Godot;
using R3;

namespace fms.Faction;

/// <summary>
/// Lv3: ショップフェイズ開始時、スクラップ を持つミニオンのレベルを 1 上げる
/// Lv5: レベルが最大のウェポンの攻撃力を上げる (未実装)
/// </summary>
[GlobalClass]
public partial class Scrap : FactionBase
{
    public override bool IsActiveAnyEffect => Level >= 3;

    public override void _Ready()
    {
        // はじめてこのシナジーのミニオンが購入されたときに、OnNextされてしまうので、Skip(1)している
        Main.WaveState.Phase.Skip(1).Subscribe(phase =>
        {
            if (phase != WavePhase.Shop)
            {
                return;
            }

            // 発動していない場合はリターンする
            if (Level < 3)
            {
                return;
            }

            var list = new List<Minion>();
            var nodes = this.GetSiblings();
            foreach (var node in nodes)
            {
                if (node is not Minion minion)
                {
                    continue;
                }

                if (minion.IsBelongTo(FactionType.Scrap))
                {
                    if (minion.IsMaxLevel)
                    {
                        continue;
                    }

                    list.Add(minion);
                }
            }

            var rand = new Random();

            // レベルアップ対象がいない場合はリターン
            if (list.Count == 0)
            {
                return;
            }

            // スクラップを持つミニオンの中からランダムに選択
            var randomIndex = rand.Next(0, list.Count); // 0 以上 numbers.Count 未満の乱数を生成
            var target = list[randomIndex];

            // レベルを上げる
            var currentLevel = target.Level.CurrentValue;
            target.SetLevel(currentLevel + 1);
        }).AddTo(this);
    }
}