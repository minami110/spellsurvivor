using Godot;

namespace fms;

public static partial class NodeExtensions
{
    /// <summary>
    ///     Wrapper for SceneTree PhysicsFrame signal awaiter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static SignalAwaiter BeginOfPhysicsFrameAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    /// <summary>
    ///     Wrapper for SceneTree ProcessFrame signal awaiter
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

    /// <summary>
    ///     Wrapper for GetTree().CreateTimer()
    /// </summary>
    /// <param name="node"></param>
    /// <param name="timeSec"></param>
    /// <returns></returns>
    public static SignalAwaiter WaitForSecondsAsync(this Node node, float timeSec)
    {
        return node.ToSignal(node.GetTree().CreateTimer(timeSec), SceneTreeTimer.SignalName.Timeout);
    }
}