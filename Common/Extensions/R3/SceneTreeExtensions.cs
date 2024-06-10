using System.Threading;
using Godot;

namespace R3;

public static class SceneTreeExtensions
{
    public static Observable<Unit> TreeChangedAsObservable(this SceneTree tree,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => tree.TreeChanged += h,
            h => tree.TreeChanged -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> TreeProcessModeChangedAsObservable(this SceneTree tree,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => tree.TreeProcessModeChanged += h,
            h => tree.TreeProcessModeChanged -= h,
            cancellationToken
        );
    }
}