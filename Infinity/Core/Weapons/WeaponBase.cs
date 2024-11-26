using System;
using System.Runtime.CompilerServices;
using fms.Faction;
using fms.Projectile;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

public static class WeaponAttributeNames
{
    public const string DamageRate = "DamageRate";
    public const string SpeedRate = "SpeedRate";
}

/// <summary>
/// Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D, IAttributeDictionary
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
            if (IsNodeReady())
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

    /// <summary>
    /// クールダウン, アニメーションどちらにも作用する速度倍率
    /// </summary>
    [Export(PropertyHint.Range, "0.001,2,,or_greater")]
    public float BaseSpeedRate { get; set; } = 1f;

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

    private readonly Dictionary<string, Variant> _attributes = new();

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
    [Obsolete]
    public uint SolvedCoolDownFrame
    {
        get
        {
            var coolDown = (uint)Mathf.Ceil(BaseCoolDownFrame / SpeedRate);
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
    /// エフェクト適用前のベースのアニメーションフレーム数
    /// </summary>
    public virtual uint BaseAnimationFrames => 1u;

    /// <summary>
    /// 武器のダメージ量 (エフェクトを適用した後の値)
    /// </summary>
    public uint Damage { get; private set; }

    public float SpeedRate { get; private set; }

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
            SpeedRate = BaseSpeedRate;
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
        }
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


        FrameTimer.WaitFrame = (uint)Mathf.Ceil(BaseCoolDownFrame / SpeedRate);
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


    /// <summary>
    /// 武器が起動したときに呼び出されるメソッド, 通常はバトルウェーブ開始時に呼ばれる
    /// </summary>
    private protected virtual void OnStartAttack()
    {
    }

    private protected virtual void OnUpdateAnyAttribute(Dictionary<string, Variant> attributes)
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
        FrameTimer.WaitFrame = (uint)Mathf.Ceil(BaseCoolDownFrame / SpeedRate);
        FrameTimer.Start();
    }

    private void OnUpdateAnyAttribute()
    {
        // Damage
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.DamageRate, out var v))
            {
                var damage = BaseDamage + BaseDamage * (float)v;
                Damage = (uint)damage;
            }
        }

        // Speed
        {
            if (_attributes.TryGetValue(WeaponAttributeNames.SpeedRate, out var v))
            {
                var speedRate = BaseSpeedRate + (float)v;
                SpeedRate = Math.Max(speedRate, 0.001f);
            }
        }

        OnUpdateAnyAttribute(_attributes);
    }

    bool IAttributeDictionary.TryGetAttribute(string key, out Variant value)
    {
        return _attributes.TryGetValue(key, out value);
    }

    void IAttributeDictionary.SetAttribute(string key, Variant value)
    {
        _attributes[key] = value;
        OnUpdateAnyAttribute();
    }
}