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
    [Export]
    private WeaponConfig _config = null!;

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
    public virtual uint AnimationTime => 0u;

    public WeaponState State { get; private set; } = null!;

    public WeaponConfig Config => _config;

    // Note: 継承先が気軽にオーバーライドできるようにするためにここでは _Notification で @ready などを実装している
    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            // Weapon group に所属する
            AddToGroup(Constant.GroupNameWeapon);

            // Create WeaponState
            State = new WeaponState(
                1u,
                _config.Damage,
                _config.CooldownTime,
                _config.AnimationSpeedRate * 0.01f,
                _config.Knockback
            );
            AddChild(State);

            // FrameTimer が存在していなかったら作成する
            {
                var frametimer = this.FindFirstChild<FrameTimer>();
                if (frametimer is null)
                {
                    frametimer = new FrameTimer();
                    AddChild(frametimer);
                }

                FrameTimer = frametimer;
            }

            // 武器のクールダウンが完了時のコールバックを登録
            FrameTimer.TimeOut
                .Subscribe(this, (_, self) => { self.OnCoolDownCompleted(self.State.Level.CurrentValue); })
                .AddTo(this);

            // ToDo: 仮
            // 手前に見えるようにする
            ZIndex = 10;

            // 親が IEntity であることを確認しこの武器の所有者として設定する
            var parent = GetParentOrNull<IEntity>();
            OwnedEntity = parent ?? throw new ApplicationException("WeaponBase は IEntity の子ノードでなければなりません");
        }
        else if (what == NotificationReady)
        {
            // 兄弟に Faction を作成またはレベルアップ
            CreateFactions();

            if (AutoStart)
            {
                StartAttack();
            }
            else
            {
                StopAttack();
            }
        }
        else if (what == NotificationExitTree)
        {
            var sibs = GetParent().GetChildren();
            foreach (var n in sibs)
            {
                if (n is not FactionBase f)
                {
                    continue;
                }

                var type = f.GetFactionType();
                if (IsBelongTo(type))
                {
                    f.Level--;
                }
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

    /// <summary>
    /// Faction (シナジー) に所属しているかどうか
    /// </summary>
    /// <param name="factionType"></param>
    /// <returns></returns>
    public bool IsBelongTo(FactionType factionType)
    {
        return _config.Faction.HasFlag(factionType);
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

        FrameTimer.WaitFrame = State.AttackSpeed.CurrentValue;
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
        OnStopAttack();
    }

    /// <summary>
    /// Shop 販売の UI 用の説明文を取得
    /// </summary>
    /// <returns></returns>
    internal virtual string GetDescriptionForShop()
    {
        var desc = _config.Description;

        desc += "\n\n";
        desc += $"Damage: {_config.Damage}\n";

        var totalFrames = _config.CooldownTime + AnimationTime;
        desc += $"Cooldown: {totalFrames}({_config.CooldownTime} + {AnimationTime}) frames\n";
        desc += $"Speed: {_config.AnimationSpeedRate}%\n";
        desc += $"Knockback: {_config.Knockback} px/s";

        return desc;
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

    private protected virtual void OnStopAttack()
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
        FrameTimer.WaitFrame = State.AttackSpeed.CurrentValue;
        FrameTimer.Start();
    }

    // 所属している Faction を作成する
    private void CreateFactions()
    {
        foreach (var faction in FactionUtil.GetFactionTypes())
        {
            if (!IsBelongTo(faction))
            {
                continue;
            }

            var f = FactionUtil.CreateFaction(faction);
            f.Level++; // 1

            // 呼び出しタイミングによっては Parent が busy なので CallDeferred で追加する
            // 重複して Faction が存在する場合, Faction 側で勝手に合体するのでこちらでは気にしない
            CallDeferred(Node.MethodName.AddSibling, f);
        }
    }

    private void LevelDownFactions()
    {
        foreach (var faction in FactionUtil.GetFactionTypes())
        {
            if (!IsBelongTo(faction))
            {
                continue;
            }

            // すでに Faction が存在していたらレベルを下げる
            var s = this.FindSibling("*", faction.ToString());
            if (s.Count > 0)
            {
                var f = (FactionBase)s[0];
                f.Level--;
            }
        }
    }
}