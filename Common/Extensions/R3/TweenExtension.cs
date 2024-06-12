using System.Threading;
using Godot;

namespace R3;

public static class TweenExtensions
{
    public static Observable<Unit> FinishedAsObservable(this Tween tween,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => tween.Finished += h,
            h => tween.Finished -= h,
            cancellationToken
        );
    }
}