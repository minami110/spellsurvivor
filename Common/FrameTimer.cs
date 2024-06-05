using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class FrameTimer : Node
{
    [Export(PropertyHint.Range, "1,99999")]
    private uint _waitFrame = 20;

    [Export]
    private bool _autostart;

    private readonly ReactiveProperty<int> _frameLeft = new(-1);

    private readonly Subject<Unit> _timeOut = new();

    public Observable<Unit> TimeOut => _timeOut;

    public ReadOnlyReactiveProperty<int> FrameLeft => _frameLeft;

    public uint WaitFrame
    {
        get => _waitFrame;
        set => _waitFrame = value;
    }

    public bool IsStopped => _frameLeft.Value <= 0;

    public override void _Notification(int what)
    {
        if (what == NotificationReady)
        {
            Disposable.Combine(_timeOut, _frameLeft).AddTo(this);
            if (_autostart)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
        else if (what == NotificationProcess)
        {
            var nextFrameLeft = _frameLeft.Value - 1;

            if (nextFrameLeft < 0)
            {
                _timeOut.OnNext(Unit.Default);
                _frameLeft.Value = (int)_waitFrame;
            }
            else
            {
                _frameLeft.Value = nextFrameLeft;
            }
        }
    }

    public void Start()
    {
        _frameLeft.Value = (int)_waitFrame;
        SetProcess(true);
    }

    public void Stop()
    {
        SetProcess(false);
        _frameLeft.Value = -1;
    }
}