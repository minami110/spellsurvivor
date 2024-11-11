using System;
using System.Threading;
using Godot;

namespace R3;

public static class NodeExtensions
{
    /// <summary>
    /// Dispose self on target node has bee tree exited.
    /// </summary>
    /// <param name="disposable"></param>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Self disposable</returns>
    public static T AddTo<T>(this T disposable, Node node) where T : IDisposable
    {
        // Note: Dispose when tree exited, so if node is not inside tree, dispose immediately.
        if (!node.IsInsideTree())
        {
            if (!node.IsNodeReady()) // Before enter tree
            {
                GD.PrintErr("AddTo does not support to use before enter tree.");
            }

            disposable.Dispose();
            return disposable;
        }

        node.TreeExited += () => disposable.Dispose();
        return disposable;
    }

    public static Observable<Node> ChildEnteredTreeAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Node.ChildEnteredTreeEventHandler, Node>(
            h => new Node.ChildEnteredTreeEventHandler(h),
            h => node.ChildEnteredTree += h,
            h => node.ChildEnteredTree -= h,
            cancellationToken
        );
    }

    public static Observable<Node> ChildExtingTreeAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Node.ChildExitingTreeEventHandler, Node>(
            h => new Node.ChildExitingTreeEventHandler(h),
            h => node.ChildExitingTree += h,
            h => node.ChildExitingTree -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> ChildOrderChangedAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.ChildOrderChanged += h,
            h => node.ChildOrderChanged -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> TreeEnteredAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.TreeEntered += h,
            h => node.TreeEntered -= h,
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

    public static Observable<Unit> TreeExitingAsObservable(this Node node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.TreeExiting += h,
            h => node.TreeExiting -= h,
            cancellationToken
        );
    }
}