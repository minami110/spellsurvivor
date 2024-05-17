using System.Threading;
using Godot;
using R3;

namespace spellsurvivor;

public static class CollisionObject2DExtensions
{
    public static Observable<Unit> MouseEnteredAsObservable(this CollisionObject2D node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.MouseEntered += h,
            h => node.MouseEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> MouseExitedAsObservable(this CollisionObject2D node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.MouseExited += h,
            h => node.MouseExited -= h,
            cancellationToken
        );
    }

    public static Observable<long> MouseShapeEnteredAsObservable(this CollisionObject2D node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<CollisionObject2D.MouseShapeEnteredEventHandler, long>(
            h => new CollisionObject2D.MouseShapeEnteredEventHandler(h),
            h => node.MouseShapeEntered += h,
            h => node.MouseShapeEntered -= h,
            cancellationToken
        );
    }

    public static Observable<long> MouseShapeExitedAsObservable(this CollisionObject2D node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<CollisionObject2D.MouseShapeExitedEventHandler, long>(
            h => new CollisionObject2D.MouseShapeExitedEventHandler(h),
            h => node.MouseShapeExited += h,
            h => node.MouseShapeExited -= h,
            cancellationToken
        );
    }
}