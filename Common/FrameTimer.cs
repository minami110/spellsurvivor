using System;
using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class FrameTimer : Node
{
    /// <summary>
    /// タイマーの待機フレームを設定, 1 以上の値を設定してください
    /// </summary>
    /// <remarks>
    /// タイマーが稼働中の場合は, 次のループからここで設定した値で待機します
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [Export(PropertyHint.Range, "1,216000")] // 216000f = 1 hour
    public uint WaitFrame
    {
        get => _waitFrame;
        set
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(WaitFrame),
                    $"[{nameof(FrameTimer)}] {nameof(WaitFrame)} must be greater than 0"
                );
            }

            _waitFrame = value;
        }
    }

    /// <summary>
    /// Ready 時に自動でタイマーをスタートするかどうか
    /// </summary>
    [Export]
    private bool _autostart;

    private readonly ReactiveProperty<uint> _frameLeft = new(0);
    private readonly Subject<Unit> _timeOut = new();
    private bool _paused;
    private uint _waitFrame = 20u;

    /// <summary>
    /// タイマーがタイムアウトした時に発行されるイベント
    /// </summary>
    public Observable<Unit> TimeOut => _timeOut;

    /// <summary>
    /// タイマーの残りフレーム, 停止している場合は 0 が返ります
    /// </summary>
    public ReadOnlyReactiveProperty<uint> FrameLeft => _frameLeft;

    public bool Paused
    {
        get => _paused;
        set
        {
            if (_paused == value)
            {
                return;
            }

            _paused = value;
            SetPhysicsProcess(!_paused);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationReady)
        {
            if (_autostart)
            {
                Start();
                _autostart = false;
            }
        }
        else if (what == NotificationPhysicsProcess)
        {
            var next = 0u;
            if (_frameLeft.Value == 0)
            {
                GD.PrintErr("FrameLeft is 0");
            }
            else
            {
                next = _frameLeft.Value - 1;
            }

            if (next == 0)
            {
                _timeOut.OnNext(Unit.Default);
                _frameLeft.Value = WaitFrame;
            }
            else
            {
                _frameLeft.Value = next;
            }
        }
        else if (what == NotificationExitTree)
        {
            _frameLeft.Dispose();
            _timeOut.Dispose();
        }
    }

    public void Start()
    {
        _frameLeft.Value = WaitFrame;
        SetPhysicsProcess(true);
    }

    public void Stop()
    {
        _frameLeft.Value = 0;
        SetPhysicsProcess(false);
        _autostart = false;
    }
}