using System;
using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class FrameTimer : Node
{
    [Export(PropertyHint.Range, "1,99999")]
    private int _waitFrame = 20;

    private readonly ReactiveProperty<int> _frameLeft = new(-1);

    private readonly Subject<Unit> _timeOut = new();

    public Observable<Unit> TimeOut => _timeOut;

    public ReadOnlyReactiveProperty<int> FrameLeft => _frameLeft;

    public int WaitFrame
    {
        get => _waitFrame;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("WaitFrame must be greater than or equal to 0");
            }

            _waitFrame = value;
        }
    }

    public bool IsStopped => _frameLeft.Value <= 0;

    public override void _Notification(int what)
    {
        if (what == NotificationReady)
        {
            Stop();
            Disposable.Combine(_timeOut, _frameLeft).AddTo(this);
        }
        else if (what == NotificationProcess)
        {
            var nextFrameLeft = _frameLeft.Value - 1;

            if (nextFrameLeft < 0)
            {
                _timeOut.OnNext(Unit.Default);
                _frameLeft.Value = _waitFrame;
            }
            else
            {
                _frameLeft.Value = nextFrameLeft;
            }
        }
    }

    public void Start()
    {
        GD.Print("Timer Start");
        _frameLeft.Value = _waitFrame;
        SetProcess(true);
    }

    public void Stop()
    {
        GD.Print("Timer Stop");
        SetProcess(false);
        _frameLeft.Value = -1;
    }
}