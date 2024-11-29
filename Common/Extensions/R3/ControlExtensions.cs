using System.Threading;
using Godot;
using R3;

namespace fms;

public static partial class NodeExtensions
{
    public static Observable<Unit> FocusEnteredAsObservable(this Control node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.FocusEntered += h,
            h => node.FocusEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> FocusExitedAsObservable(this Control node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.FocusExited += h,
            h => node.FocusExited -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> MouseEnteredAsObservable(this Control node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.MouseEntered += h,
            h => node.MouseEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> MouseExitedAsObservable(this Control node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.MouseExited += h,
            h => node.MouseExited -= h,
            cancellationToken
        );
    }
}