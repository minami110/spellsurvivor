using Godot;
using Godot.Collections;

namespace fms;

public static partial class NodeExtensions
{
    /// <summary>
    /// Wrapper for SceneTree PhysicsFrame signal awaiter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static SignalAwaiter BeginOfPhysicsFrameAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    /// <summary>
    /// Wrapper for SceneTree ProcessFrame signal awaiter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static SignalAwaiter BeginOfProcessAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    public static void DebugLog(this Node node, string message)
    {
        GD.Print($"[{node.GetType()}] {message}");
    }

    public static T? FindFirstChild<T>(this Node node)
        where T : Node
    {
        var size = node.GetChildCount();
        for (var i = 0; i < size; i++)
        {
            if (node.GetChild(i) is T t)
            {
                return t;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first parent node of the specified type.
    /// </summary>
    /// <param name="node">The starting node from which to search for a parent of type T.</param>
    /// <typeparam name="T">The type of the parent node to find.</typeparam>
    /// <returns>The first parent node of the specified type, or null if no such parent is found.</returns>
    public static T? FindInParent<T>(this Node node)
        where T : Node
    {
        var parent = node.GetParent();

        while (parent is not null)
        {
            if (parent is T t)
            {
                return t;
            }

            parent = parent.GetParent();
        }

        return null;
    }

    /// <summary>
    /// Finds sibling nodes with the specified pattern and type. GetParent().FindChildren() wrapper.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="pattern"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Array<Node> FindSibling(this Node node, string pattern, string type = "")
    {
        return node.GetParent().FindChildren(pattern, type, false, false);
    }

    public static Array<Node> GetSiblings(this Node node)
    {
        return node.GetParent().GetChildren();
    }

    public static SignalAwaiter NextFrameAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    public static SignalAwaiter NextPhysicsFrameAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    /// <summary>
    /// Wrapper for GetTree().CreateTimer()
    /// </summary>
    /// <param name="node"></param>
    /// <param name="timeSec"></param>
    /// <returns></returns>
    public static SignalAwaiter WaitForSecondsAsync(this Node node, double timeSec)
    {
        return node.ToSignal(node.GetTree().CreateTimer(timeSec), SceneTreeTimer.SignalName.Timeout);
    }
}