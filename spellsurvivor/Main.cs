#nullable enable
using System;
using Godot;

namespace spellsurvivor;

public partial class Main : Node
{
    private static Main? _instance;
    
    private Player _player = null!;
    
    public override void _Ready()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        else
        {
            throw new AggregateException("Main instance already exists");
        }
        
        _player = GetNode<Player>("Player");
    }
    
    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static Vector2 GetPlayerGlobalPosition()
    {
        if (_instance is not null)
        {
            return _instance._player.GlobalPosition;
        }

        throw new ApplicationException("Main instance is null");
    }
}