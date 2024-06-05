using Godot;
using System;

public partial class BookShaft : Node2D
{
	/// <summary>
	/// 1秒あたりの回転数
	/// </summary>
	public float RotatePerSecond { get; set; } = 360f;
	
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RotationDegrees += (float)(delta * (1 / RotatePerSecond) * 360f);
	}
}
