using System;
using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class FrameTimer : Node
{
    [Export(PropertyHint.Range, "1,9999")]
    private int _waitFrame = 20;

    private readonly Subject<Unit> _timeOut = new();
    private int _frameLeft = -1;

    public Observable<Unit> TimeOut => _timeOut;

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

    public int FrameLeft => _frameLeft > 0 ? _frameLeft : 0;

    public bool IsStopped => _frameLeft <= 0;

    public override void _EnterTree()
    {
        Stop();
    }

    public override void _Process(double delta)
    {
        _frameLeft--;

        if (_frameLeft < 0)
        {
            _timeOut.OnNext(Unit.Default);
            _frameLeft += _waitFrame;
        }
    }

    public override void _ExitTree()
    {
        _timeOut.Dispose();
    }

    public void Start()
    {
        _frameLeft = _waitFrame;
        SetProcess(true);
    }

    public void Stop()
    {
        _frameLeft = -1;
        SetProcess(false);
    }
}