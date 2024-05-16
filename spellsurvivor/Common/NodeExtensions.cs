using Godot;

namespace spellsurvivor;

public static partial class NodeExtensions
{
    /// <summary>
    /// Wrapper for GetTree().CreateTimer()
    /// </summary>
    /// <param name="node"></param>
    /// <param name="timeSec"></param>
    /// <returns></returns>
    public static SignalAwaiter WaitForSeconds(this Node node, float timeSec)
    {
        return node.ToSignal(node.GetTree().CreateTimer(timeSec), SceneTreeTimer.SignalName.Timeout);
    }
}