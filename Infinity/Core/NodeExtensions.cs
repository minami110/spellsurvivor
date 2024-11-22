using System;
using Godot;

namespace fms;

public static partial class NodeExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static Node GetPlayerNode(this Node node)
    {
        var playerNode = node.GetTree().GetFirstNodeInGroup(GroupNames.Player);
        if (playerNode is null)
        {
            throw new ApplicationException("Player not found in tree");
        }

        return playerNode;
    }
}