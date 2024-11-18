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
        get;
        set
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(WaitFrame),
                    $"[{nameof(FrameTimer)}] {nameof(WaitFrame)} must be greater than 0"
                );
            }

            field = value;
        }
    } = 20u;

    /// <summary>
    /// Ready 時に自動でタイマーをスタートするかどうか
    /// </summary>
    [Export]
    private bool _autostart;


    /// <summary>
    /// </summary>
    [Export]
    public bool OneShot { get; set; }

    private readonly ReactiveProperty<uint> _frameLeft = new(0);
    private readonly Subject<Unit> _timeOut = new();

    /// <summary>
    /// タイマーがタイムアウトした時に発行されるイベント
    /// </summary>
    public Observable<Unit> TimeOut => _timeOut;

    /// <summary>
    /// タイマーの残りフレーム, 停止している場合は 0 が返ります
    /// </summary>
    public ReadOnlyReactiveProperty<uint> FrameLeft => _frameLeft;

    /// <summary>
    /// チャイマーの一時停止状態を示します
    /// </summary>
    /// <remarks>
    /// true に設定するとタイマーが一時停止し、false に設定すると再開します
    /// </remarks>
    public bool Paused
    {
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            SetPhysicsProcess(!field);
        }
    }

    /// <summary>
    /// このプロパティはタイマーが停止しているかどうかを判定します。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public bool IsStopped => _frameLeft.Value == 0u;

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
            var current = _frameLeft.Value;
            if (current == 0u)
            {
                // Note: FrameTimer の PhysicsProcess が Start() 以外の方法で有効にされた場合にここに入る
                throw new InvalidProgramException("Please call Start() method before using FrameTimer");
            }

            var next = current - 1u;
            if (next == 0u)
            {
                if (OneShot)
                {
                    Stop();
                    _timeOut.OnNext(Unit.Default);
                }
                else
                {
                    _timeOut.OnNext(Unit.Default);
                    _frameLeft.Value = WaitFrame;
                }
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

    /// <summary>
    /// Starts or restarts the frame timer with the specified number of frames to wait.
    /// </summary>
    /// <remarks>
    /// This method sets the frame timer's countdown to the value of the <see cref="WaitFrame" /> property
    /// and enables the physics processing for the node. The frame timer will continue to count down
    /// each frame until it reaches zero, at which point it will emit a timeout signal if any observers
    /// are listening.
    /// </remarks>
    public void Start()
    {
        _frameLeft.Value = WaitFrame;
        SetPhysicsProcess(true);
    }

    /// <summary>
    /// Stops the frame timer, setting the remaining frames to zero and disabling physics processing.
    /// </summary>
    /// <remarks>
    /// This method sets the frame timer's countdown to zero, disables the physics processing for the node,
    /// and prevents the timer from automatically starting again if it was set to autostart.
    /// </remarks>
    public void Stop()
    {
        _frameLeft.Value = 0;
        SetPhysicsProcess(false);
        _autostart = false;
    }
}