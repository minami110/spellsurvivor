using System;
using System.Collections.Generic;
using fms.Effect;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class AorticKnife : Hocho
{
    private uint _lifestealAmount;
    private float _lifeStealRate;

    public override void _Ready()
    {
        base._Ready();

        StaticDamage.Hit.Subscribe(this, (payload, state) =>
        {
            var hitEntity = (IEntity)(Node2D)payload["Entity"];

            // ToDo: ライフスティールの対象かどうか?
            // Player なら Enemy にあたったとき みたいな確認

            // ライフスティールの確率計算
            var chance = Math.Clamp(state._lifeStealRate, 0f, 1f);
            if (GD.Randf() < chance)
            {
                // OwnedEntity (Player) を回復する
                var player = state.OwnedEntity;
                player.ApplayDamage(state._lifestealAmount * -1f, player, state);
            }
        }).AddTo(this);
    }

    private protected override void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
        _lifestealAmount = 0;
        _lifeStealRate = 0f;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                case Lifesteal lifesteal:
                {
                    _lifestealAmount += lifesteal.Amount;
                    _lifeStealRate += lifesteal.Rate;
                    break;
                }
            }
        }
    }
}