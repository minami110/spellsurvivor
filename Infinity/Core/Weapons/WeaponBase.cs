using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    /// 武器の基礎ダメージ量
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1")]
    public float BaseDamage { get; set; } = 10f;

    /// <summary>
    /// 武器の Cooldown にかかる基礎フレーム数
    /// </summary>
    [Export(PropertyHint.Range, "1,9999,1,suffix:frames")]
    public uint BaseCoolDownFrame
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            // 実行中の EnterTree 後のみ実行する
            if (!Engine.IsEditorHint() && IsNodeReady())
            {
                // Update Frame Timer
                FrameTimer.WaitFrame = SolvedCoolDownFrame;
            }
        }
    } = 10u;

    /// <summary>
    /// 武器が敵に与えるノックバック量
    /// </summary>
    [Export(PropertyHint.Range, "0,999,1,suffix:px/s")]
    public uint Knockback { get; set; } = 20u;

    // ---------- Animation Parameters ----------

    /// <summary>
    /// ToDo: こっちでもっておくべきか検討する
    /// <see cref="WeaponPositionAnimator" /> により自動で位置を調整するかどうか
    /// </summary>
    [ExportGroup("Animation")]
    [Export]
    public bool AutoPositioning { get; set; } = true;

    // ---------- Debug Parameters ----------

    /// <summary>
    /// Tree に入った時に自動で Start するかどうか
    /// </summary>
    /// <remarks>
    /// 開発用のパラメーター, ショップから購入されたときは false が代入される
    /// デフォで true にしておくことで, ツリーに雑に配置しておいても動作するようにする
    /// </remarks>
    [ExportGroup("For Debugging")]
    [Export]
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// エディタ上でデバッグ情報を描画するかどうか
    /// </summary>
    [Export]
    public bool DrawDebugInfoInEditor { get; private set; } = true;

    /// <summary>
    /// 現在の武器の Level
    /// Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export(PropertyHint.Range, "1,5")]
    public uint Level { get; set; } = 1u;

    /// <summary>
    /// Minion が所属する Faction
    /// Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export]
    public FactionType Faction { get; set; }

    // 現在武器に付与されている Effect
    private readonly HashSet<EffectBase> _effects = [];

    // クールダウンの削減率 (範囲: 0 ~ 1 / デフォルト: 0)
    private float _coolDownReduceRate;

    // Effect の変更があったかどうか
    private bool _isDirtyEffect;

    /// <summary>
    /// 武器の Id
    /// Note: Minion から勝手に代入されます
    /// </summary>
    public string MinionId { get; internal set; } = string.Empty;

    /// <summary>
    /// この武器を所有している Entity
    /// </summary>
    public IEntity OwnedEntity { get; private set; } = null!;

    /// <summary>
    /// Effect の解決後の Cooldown のフレーム数
    /// </summary>
    public uint SolvedCoolDownFrame
    {
        get
        {
            var coolDown = (uint)Mathf.Floor(BaseCoolDownFrame * (1f - _coolDownReduceRate));
            return Math.Max(coolDown, 1u);
        }
    }

    /// <summary>
    /// FrameTimer を取得
    /// </summary>
    private FrameTimer FrameTimer { get; set; } = null!;

    /// <summary>
    /// 次の攻撃までの残りフレームを取得
    /// </summary>
    public ReadOnlyReactiveProperty<uint> CoolDownLeft => FrameTimer.FrameLeft;

    /// <summary>
    /// 武器のダメージ量 (エフェクトを適用した後の値)
    /// </summary>
    public uint Damage { get; private set; }

    // Note: 継承先が気軽にオーバーライドできるようにするためにここでは _Notification で @ready などを実装
    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Weapon group に所属する
            AddToGroup(Constant.GroupNameWeapon);

            // FrameTimer が存在していなかったら作成する
            var frametimer = this.FindFirstChild<FrameTimer>();
            if (frametimer is null)
            {
                frametimer = new FrameTimer();
                AddChild(frametimer);
            }

            FrameTimer = frametimer;

            // 親が IEntity であることを確認しこの武器の所有者として設定する
            var parent = GetParentOrNull<IEntity>();
            OwnedEntity = parent ?? throw new ApplicationException("WeaponBase は IEntity の子ノードでなければなりません");

            // ToDo: 仮
            // 手前に見えるようにする
            ZIndex = 10;

            // ToDO: 
            // エフェクト解決が行われない場合の最終敵なステータスをここで更新しておく
            Damage = (uint)BaseDamage;
        }
        else if (what == NotificationReady)
        {
            // 武器のクールダウンが完了時のコールバックを登録
            FrameTimer.TimeOut
                .Subscribe(this, (_, state) => { state.OnCoolDownCompleted(state.Level); })
                .AddTo(this);

            if (AutoStart)
            {
                StartAttack();
            }
            else
            {
                StopAttack();
            }

            // Note: Godot では override していないと Process が動かない
            //       Notification で使いたいのでここでマニュアル有効化する
            SetProcess(true);
        }
        else if (what == NotificationProcess)
        {
            // ToDo: ショップでも裏で動いてるのへんかも?
            // こっちで対応するより, Shop にいるときは削除 みたいな上位からの実装があったほうが素直かも
            SolveEffect();
        }
    }

    public virtual void AddEffect(EffectBase effect)
    {
        _effects.Add(effect);
        _isDirtyEffect = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// 武器のクールダウンを減らすタイマーを開始 (武器の有効化)
    /// </summary>
    public void StartAttack()
    {
        if (!FrameTimer.IsStopped)
        {
            return;
        }

        FrameTimer.WaitFrame = SolvedCoolDownFrame;
        FrameTimer.OneShot = true;
        FrameTimer.Start();
        OnStartAttack();
    }

    /// <summary>
    /// 武器のクールダウンを管理するタイマーを停止, カウントをリセット (武器の無効化)
    /// </summary>
    public void StopAttack()
    {
        if (FrameTimer.IsStopped)
        {
            return;
        }

        FrameTimer.Stop();
    }

    /// <summary>
    /// 武器のクールダウンが終了した時に呼び出されるメソッド
    /// </summary>
    /// <param name="level">現在の武器のレベル</param>
    private protected virtual void OnCoolDownCompleted(uint level)
    {
        RestartCoolDown();
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
    /// 継承クラスから クールダウンの終了時に呼び出される
    /// ToDo: 引数経由のコンテキストから呼べないほうが硬いけど一旦シンプルな実装で
    /// </summary>
    private protected void RestartCoolDown()
    {
        if (!FrameTimer.IsStopped)
        {
            return;
        }

        // タイマーを再開する
        FrameTimer.OneShot = true;
        FrameTimer.WaitFrame = SolvedCoolDownFrame;
        FrameTimer.Start();
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
        var damage = (uint)BaseDamage;
        var reduceCoolDownRate = 0f;

        foreach (var effect in _effects)
        {
            switch (effect)
            {
                // Strength (武器ダメージ)
                case Strength strengthEffect:
                {
                    damage += strengthEffect.Amount;
                    break;
                }
                // クールダウンを減少させるエフェクト
                case ReduceCoolDownRate reduceCoolDownRateEffect:
                {
                    reduceCoolDownRate += reduceCoolDownRateEffect.Value;
                    break;
                }
            }
        }

        // 値を更新
        Damage = damage;
        _coolDownReduceRate = Math.Max(reduceCoolDownRate, 0);

        OnSolveEffect(_effects);
    }
}