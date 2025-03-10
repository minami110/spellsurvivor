using Godot;
using R3;

namespace fms;

public partial class PickableGoldNugget : Node2D
{
    [Export]
    private uint _amount = 1;

    [Export]
    private double _attractionSec = 0.4d;

    [Export]
    private AudioStream? _pickSound;

    private Vector2 _attractionStartPosition;
    private Tween? _idlingAnimationTween;

    private Node2D? _targetNode;

    public override void _EnterTree()
    {
        AddToGroup(Constant.GroupNamePickableItem);
    }

    public override void _Ready()
    {
        var area = GetNode<Area2D>("AttractionArea");
        area.BodyEnteredAsObservable()
            .Take(1)
            .Subscribe(StartAttraction)
            .AddTo(this);

        GetNode<PickableAnimator>("PickableAnimator").StartIdlingTween();
    }

    private void MoveToTarget(float alpha)
    {
        if (_targetNode is null)
        {
            return;
        }

        var goalPos = _targetNode.GlobalPosition;
        // 完全に同じ座標だとキショいので, ちょっと手前に着地するようにする
        goalPos -= (goalPos - _attractionStartPosition).Normalized() * 20f;

        // 開始地点を 0 ゴール地点 を 1 とした時, 現在の alpha に応じて位置を算出する
        var targetPos = _attractionStartPosition.Lerp(goalPos, alpha);
        GlobalPosition = targetPos;
    }

    private void OnPickupped()
    {
        // ToDo: Pick 時のパーティクルの再生

        // Pick 時のサウンドの再生
        if (_pickSound != null)
        {
            SoundManager.PlaySoundEffect(_pickSound);
        }

        // Player の GoldNugget を増加
        if (_targetNode is EntityPlayer player)
        {
            player.State.AddGoldNugget(_amount);
        }

        QueueFree();
    }

    private void StartAttraction(Node2D node)
    {
        if (_targetNode is not null)
        {
            return;
        }

        GetNode<PickableAnimator>("PickableAnimator").StopIdlingTween();

        // アイテムを拾う対象を設定
        _targetNode = node;

        // Collision がもう必要ないので無効化しておく
        var col = GetNode<CollisionShape2D>("%CollisionShape");
        col.Visible = false; // Debug 用
        col.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

        // Attraction Tween
        if (_attractionSec <= 0d)
        {
            OnPickupped();
            return;
        }

        // アイテムの開始地点を保存しておく
        _attractionStartPosition = GlobalPosition;
        var tween = CreateTween();
        tween.TweenMethod(Callable.From((float x) => MoveToTarget(x)), 0f, 1f, _attractionSec)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);
        tween.FinishedAsObservable()
            .Take(1)
            .Subscribe(this, (_, state) => state.OnPickupped())
            .AddTo(this);
    }
}