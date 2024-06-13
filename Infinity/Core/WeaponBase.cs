using System;
using System.Collections.Generic;
using fms.Effect;
using fms.Faction;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
///     Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D
{
    /// <summary>
    ///     武器の Cooldown にかかるフレーム数 (ベース値)
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    public uint BaseCoolDownFrame { get; private set; } = 10;

    /// <summary>
    ///     Tree に入った時に自動で Start するかどうか
    /// </summary>
    [ExportGroup("For Debugging")]
    [Export]
    private bool _autostart;

    /// <summary>
    ///     現在の武器の Level
    ///     Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export(PropertyHint.Range, "1,5")]
    public uint Level { get; set; } = 1;

    /// <summary>
    ///     Minion が所属する Faction
    ///     Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export]
    public FactionType Faction { get; set; }

    /// <summary>
    ///     現在武器が持っている マナ
    /// </summary>
    [Export]
    public float Mana { get; private set; }

    /// <summary>
    ///     敵が確率で Heal を落とすようになる確率 (0, 1)
    /// </summary>
    [Export]
    public float EnemyDropSmallHealRate { get; private set; }

    // 現在武器に付与されている Effect
    private readonly HashSet<EffectBase> _effects = new();

    // クールダウンの削減率 (範囲: 0 ~ 1 / デフォルト: 0)
    private float _coolDownReduceRateRp;

    // Effect の変更があったかどうか
    private bool _isDirtyEffect;

    // マナの生成にかかるフレーム数 (0 の場合は生成しない)
    private int _manaGenerationInterval;

    // マナの生成量
    private int _manaGenerationValue;

    /// <summary>
    ///     武器の Id
    ///     Note: Minion から勝手に代入されます
    /// </summary>
    public string MinionId { get; set; } = string.Empty;

    /// <summary>
    ///     Effect の解決後の Cooldown のフレーム数
    /// </summary>
    public uint SolvedCoolDownFrame
    {
        get
        {
            var coolDown = (uint)Mathf.Floor(BaseCoolDownFrame * (1f - _coolDownReduceRateRp));
            return Math.Max(coolDown, 1u);
        }
    }

    private FrameTimer FrameTimer => GetNode<FrameTimer>("FrameTimer");

    /// <summary>
    ///     次の攻撃までの残りフレーム
    /// </summary>
    public ReadOnlyReactiveProperty<int> CoolDownLeft => FrameTimer.FrameLeft;

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            Name = $"(Weapon) {MinionId}";
            if (!IsInGroup(Constant.GroupNameWeapon))
            {
                AddToGroup(Constant.GroupNameWeapon);
            }
        }
        else if (what == NotificationReady)
        {
            FrameTimer.TimeOut
                .Subscribe(this, (_, state) => { state.DoAttack(state.Level); })
                .AddTo(this);

            if (_autostart)
            {
                StartAttack();
            }
            else
            {
                StopAttack();
            }

            // Note: Process を override していないのでここで手動で有効化する
            SetProcess(true);
        }
        else if (what == NotificationProcess)
        {
            SolveEffect();
            GenerateMana();
        }
    }

    public virtual void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
        _isDirtyEffect = true;
    }

    public bool IsBelongTo(FactionType factionType)
    {
        return Faction.HasFlag(factionType);
    }

    public void StartAttack()
    {
        FrameTimer.WaitFrame = SolvedCoolDownFrame;
        FrameTimer.Start();
    }

    public void StopAttack()
    {
        FrameTimer.Stop();
    }

    private protected virtual void DoAttack(uint level)
    {
    }

    private protected virtual void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
    }

    private void GenerateMana()
    {
        if (_manaGenerationInterval == 0 || _manaGenerationValue == 0)
        {
            return;
        }

        // Gets current frame
        var frame = GetTree().GetFrame();
        if (frame % _manaGenerationInterval == 0)
        {
            Mana += _manaGenerationValue;
        }
    }

    private void SolveEffect()
    {
        if (_effects.Count == 0)
        {
            return;
        }

        // Dispose されたエフェクトを削除
        var count = _effects.RemoveWhere(effect => effect.IsDisposed);
        if (count > 0)
        {
            _isDirtyEffect = true;
        }

        if (!_isDirtyEffect)
        {
            return;
        }

        _isDirtyEffect = false;

        // 値を初期化する
        var reduceCoolDownRate = 0f;
        var manaRegenerationValue = 0;
        var manaRegenerationInterval = 0;
        var enemyDropSmallHealRate = 0f;

        foreach (var effect in _effects)
        {
            switch (effect)
            {
                // クールダウンを減少させるエフェクト
                case ReduceCoolDownRate reduceCoolDownRateEffect:
                {
                    reduceCoolDownRate += reduceCoolDownRateEffect.Value;
                    break;
                }
                // 常にマナを生成するエフェクト
                case AddManaRegeneration addManaRegeneration:
                    manaRegenerationValue += addManaRegeneration.Value;
                    manaRegenerationInterval += addManaRegeneration.Interval;
                    break;
                // 敵が確率で Heal を落とすようになる Effect
                case EnemyDropSmallHeal enemyDropHealItem:
                    enemyDropSmallHealRate += enemyDropHealItem.Rate;
                    break;
            }
        }

        // 値を更新
        _coolDownReduceRateRp = Math.Max(reduceCoolDownRate, 0);
        _manaGenerationInterval = Math.Max(manaRegenerationInterval, 0);
        _manaGenerationValue = Math.Max(manaRegenerationValue, 0);
        EnemyDropSmallHealRate = Math.Max(enemyDropSmallHealRate, 0f);

        OnSolveEffect(_effects);
    }
}