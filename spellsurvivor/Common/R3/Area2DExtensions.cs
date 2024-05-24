using System.Threading;
using Godot;
using R3;

namespace fms;

public static class Area2DExtensions
{
    public static Observable<Area2D> AreaEnteredAsObservable(this Area2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Area2D.AreaEnteredEventHandler, Area2D>(
            h => new Area2D.AreaEnteredEventHandler(h),
            h => area2D.AreaEntered += h,
            h => area2D.AreaEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Area2D> AreaExitedAsObservable(this Area2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Area2D.AreaExitedEventHandler, Area2D>(
            h => new Area2D.AreaExitedEventHandler(h),
            h => area2D.AreaExited += h,
            h => area2D.AreaExited -= h,
            cancellationToken
        );
    }

    public static Observable<Node2D> BodyEnteredAsObservable(this Area2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Area2D.BodyEnteredEventHandler, Node2D>(
            h => new Area2D.BodyEnteredEventHandler(h),
            h => area2D.BodyEntered += h,
            h => area2D.BodyEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Node2D> BodyExitedAsObservable(this Area2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Area2D.BodyExitedEventHandler, Node2D>(
            h => new Area2D.BodyExitedEventHandler(h),
            h => area2D.BodyExited += h,
            h => area2D.BodyExited -= h,
            cancellationToken
        );
    }
}