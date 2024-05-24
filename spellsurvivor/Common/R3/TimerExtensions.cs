using System.Threading;
using R3;
using Timer = Godot.Timer;

namespace fms;

public static class TimerExtensions
{
    public static Observable<Unit> TimeoutAsObservable(this Timer timer,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => timer.Timeout += h,
            h => timer.Timeout -= h,
            cancellationToken
        );
    }
}