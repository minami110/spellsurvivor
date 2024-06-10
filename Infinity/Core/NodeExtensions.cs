using System;
using Godot;

namespace fms;

public static partial class NodeExtensions
{
    /// <summary>
    ///     Wrapper for SceneTree PhysicsFrame signal awaiter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static Node GetPlayerNode(this Node node)
    {
        var playerNode = node.GetTree().GetFirstNodeInGroup(Constant.GroupNamePlayer);
        if (playerNode is null)
        {
            throw new ApplicationException("Player not found in tree");
        }

        return playerNode;
    }
}