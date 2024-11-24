using System.Threading;
using Godot;
using R3;

namespace fms;

public static class RigidBody2DExtensions
{
    public static Observable<Node> BodyEnteredAsObservable(this RigidBody2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<RigidBody2D.BodyEnteredEventHandler, Node>(
            h => new RigidBody2D.BodyEnteredEventHandler(h),
            h => area2D.BodyEntered += h,
            h => area2D.BodyEntered -= h,
            cancellationToken
        );
    }

    public static Observable<Node> BodyExitedAsObservable(this RigidBody2D area2D,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<RigidBody2D.BodyExitedEventHandler, Node>(
            h => new RigidBody2D.BodyExitedEventHandler(h),
            h => area2D.BodyExited += h,
            h => area2D.BodyExited -= h,
            cancellationToken
        );
    }
}