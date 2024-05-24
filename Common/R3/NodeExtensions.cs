using System.Threading;
using Godot;
using R3;

namespace fms;

public static partial class NodeExtensions
{
    public static Observable<Unit> TreeEnteredAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.TreeEntered += h,
            h => node.TreeEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> TreeExitingAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.TreeExiting += h,
            h => node.TreeExiting -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> TreeExitedAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.TreeExited += h,
            h => node.TreeExited -= h,
            cancellationToken
        );
    }
}