using System;
using System.Runtime.CompilerServices;
using fms.Faction;
using fms.Projectile;
using Godot;
using Godot.Collections;
using R3;

namespace fms;

/// <summary>
/// Weapon のベースクラス
/// </summary>
public partial class WeaponBase : Node2D
{
    [ExportGroup("Base Status")]
    [Export(PropertyHint.Range, "0,10,")]
    private uint _level = 1u;

    [Export(PropertyHint.Range, "0,10,")]
    private uint _damage = 10u;

    [Export(PropertyHint.Range, "0,10,,suffix:frames")]
    private uint _cooldown = 10u;

    [Export(PropertyHint.Range, "0,100,0.1,suffix:%")]
    private float _cooldownRate = 100f;

    [Export(PropertyHint.Range, "0,10,,suffix:px/s")]
    private uint _knockback = 20u;

    // ---------- Animation Parameters ----------

    /// <summary>
    /// <see cref="WeaponPositionAnimator" /> により自動で位置を調整するかどうか
    /// </summary>
    [ExportGroup("Animation")]
    [Export]
    public bool AutoPositioning { get; private set; } = true;

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
    /// Minion が所属する Faction
    /// Note: 通常は Minion から勝手に代入されます, Editor 直接配置での Debug 用です
    /// </summary>
    [Export]
    public FactionType Faction { get; set; }

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
    public virtual uint BaseAnimationFrames => 0u;

    public WeaponState State { get; private set; } = null!;

    // Note: 継承先が気軽にオーバーライドできるようにするためにここでは _Notification で @ready などを実装
    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Weapon group に所属する
            AddToGroup(Constant.GroupNameWeapon);

            // Create WeaponState
            State = new WeaponState(
                _level,
                _damage,
                _cooldown,
                _cooldownRate * 0.01f,
                _knockback
            );

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
        }
        else if (what == NotificationReady)
        {
            // 武器のクールダウンが完了時のコールバックを登録
            FrameTimer.TimeOut
                .Subscribe(this, (_, state) => { state.OnCoolDownCompleted(state.State.Level.CurrentValue); })
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

        FrameTimer.WaitFrame = State.Cooldown.CurrentValue;
        FrameTimer.OneShot = true;
        FrameTimer.Start();
        OnStartAttack(State.Level.CurrentValue);
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
    private protected virtual void OnStartAttack(uint level)
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
        FrameTimer.WaitFrame = State.Cooldown.CurrentValue;
        FrameTimer.Start();
    }
}