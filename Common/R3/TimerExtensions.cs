using System.Threading;
using Timer = Godot.Timer;

namespace R3;

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