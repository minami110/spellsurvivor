using System;
using System.Collections.Generic;
using fms.Effect;
using fms.Faction;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

/// <summary>
/// Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D
{
    /// <summary>
    /// 武器の Cooldown にかかるフレーム数 (ベース値)
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1")]
    public uint BaseCoolDownFrame
    {
        get => _baseCoolDownFrame;
        private protected set
        {
            if (_baseCoolDownFrame == value)
            {
                return;
            }

            _baseCoolDownFrame = value;

            if (IsNodeReady())
            {
                // Update Frame Timer
                FrameTimer.WaitFrame = SolvedCoolDownFrame;
            }
        }
    }

    /// <summary>
    /// Tree に入った時に自動で Start するかどうか (Debug 用のパラメーター, 通常は Wave に操作されるが)
    /// </summary>
    [ExportGroup("For Debugging")]
    [Export]
    private bool _autostart;

    /// <summary>
    /// 現在の武器の Level
    /// Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export(PropertyHint.Range, "1,5")]
    public uint Level { get; set; } = 1;

    /// <summary>
    /// Minion が所属する Faction
    /// Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export]
    public FactionType Faction { get; set; }

    /// <summary>
    /// 現在武器が持っている マナ
    /// </summary>
    [Export]
    public float Mana { get; private set; }


    private static readonly NodePath FrameTimerPath = new("FrameTimer");

    // 現在武器に付与されている Effect
    private readonly HashSet<EffectBase> _effects = new();
    private uint _baseCoolDownFrame = 10u;

    // クールダウンの削減率 (範囲: 0 ~ 1 / デフォルト: 0)
    private float _coolDownReduceRateRp;

    // Effect の変更があったかどうか
    private bool _isDirtyEffect;

    // マナの生成にかかるフレーム数 (0 の場合は生成しない)
    private int _manaGenerationInterval;

    // マナの生成量
    private int _manaGenerationValue;

    /// <summary>
    /// 武器の Id
    /// Note: Minion から勝手に代入されます
    /// </summary>
    public string MinionId { get; set; } = string.Empty;

    /// <summary>
    /// Effect の解決後の Cooldown のフレーム数
    /// </summary>
    public uint SolvedCoolDownFrame
    {
        get
        {
            var coolDown = (uint)Mathf.Floor(BaseCoolDownFrame * (1f - _coolDownReduceRateRp));
            return Math.Max(coolDown, 1u);
        }
    }

    /// <summary>
    /// FrameTimer を取得
    /// </summary>
    protected FrameTimer FrameTimer => GetNode<FrameTimer>(FrameTimerPath);

    /// <summary>
    /// 次の攻撃までの残りフレームを取得
    /// </summary>
    public ReadOnlyReactiveProperty<uint> CoolDownLeft => FrameTimer.FrameLeft;

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            Name = $"(Weapon) {MinionId}";

            // Weapon group に所属する
            AddToGroup(Constant.GroupNameWeapon);

            // Add FrameTimer
            if (GetNodeOrNull<FrameTimer>(FrameTimerPath) == null)
            {
                var frameTimer = new FrameTimer();
                frameTimer.Name = FrameTimerPath.ToString();
                AddChild(frameTimer);
            }
        }
        else if (what == NotificationReady)
        {
            FrameTimer.TimeOut
                .Subscribe(this, (_, state) => { state.SpawnProjectile(state.Level); })
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

    public void AddProjectile(BaseProjectile projectile)
    {
        // Note: FrameTimer が Node 継承 (座標がない) かつ必ず存在しているのでその子にスポーンする
        FrameTimer.AddChild(projectile);
    }

    public void AddProjectile(BaseProjectile projectile, Vector2 position)
    {
        projectile.Position = position;
        AddProjectile(projectile);
    }

    public void AddProjectile(BaseProjectile projectile, Vector2 position, Vector2 direction)
    {
        projectile.Position = position;
        projectile.Direction = direction;
        AddProjectile(projectile);
    }

    public void AddProjectile(BaseProjectile projectile, Vector2 position, float rotation)
    {
        projectile.Position = position;
        projectile.Rotation = rotation;
        AddProjectile(projectile);
    }

    public bool IsBelongTo(FactionType factionType)
    {
        return Faction.HasFlag(factionType);
    }

    public void StartAttack()
    {
        FrameTimer.WaitFrame = SolvedCoolDownFrame;
        FrameTimer.Start();
        OnStartAttack();
    }

    public void StopAttack()
    {
        FrameTimer.Stop();
    }

    private protected virtual void OnSolveEffect(IReadOnlySet<EffectBase> effects)
    {
    }

    /// <summary>
    /// 武器が起動したときに呼び出されるメソッド, 通常はバトルウェーブ開始時に呼ばれる
    /// </summary>
    private protected virtual void OnStartAttack()
    {
    }

    /// <summary>
    /// 武器のクールダウンが終了した時に呼び出されるメソッド
    /// </summary>
    /// <param name="level">現在の武器のレベル</param>
    private protected virtual void SpawnProjectile(uint level)
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
            }
        }

        // 値を更新
        _coolDownReduceRateRp = Math.Max(reduceCoolDownRate, 0);
        _manaGenerationInterval = Math.Max(manaRegenerationInterval, 0);
        _manaGenerationValue = Math.Max(manaRegenerationValue, 0);

        OnSolveEffect(_effects);
    }
}