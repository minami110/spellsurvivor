#nullable enable
using Godot;
using System;

namespace spellsurvivor;
public partial class PlayerLazyTracker : Node2D
{
	[Export]
	private Player _player = null!;

	[Export] private float _radius = 200f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Player の現在の位置を見て, その位置に向かって移動する
		var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
		var moveDistance = distance - _radius;
		if (moveDistance <= 0f)
		{
			return;
		}

		var direction = _player.GlobalPosition - GlobalPosition;
			direction = direction.Normalized();
			var moveVector = direction * moveDistance;
			GlobalPosition += moveVector;
	}
}
