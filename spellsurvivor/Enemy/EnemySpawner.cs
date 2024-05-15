#nullable enable
using Godot;

namespace spellsurvivor;
public partial class EnemySpawner : Node2D
{
	[Export]
	private PackedScene _enemyScene = null!;

	public override void _Ready()
	{
		var timer = GetNode<Timer>("EnemySpawnTimer");
		timer.Timeout += SpawnEnemy;
		timer.Start();
	}

	private void SpawnEnemy()
	{
		// pick random point on path
		var spawnPoint = GetNode<PathFollow2D>("SpawnPath/SpawnPoint");
		spawnPoint.ProgressRatio = GD.Randf();
		var spawnGlobalPosition = spawnPoint.GlobalPosition;
		
		// Spawn Enemy
		var enemy = _enemyScene.Instantiate<Enemy>();
		enemy.MoveSpeed = (float)GD.RandRange(20d, 100d);
		
		// Add scene
		GetTree().Root.AddChild(enemy);
		enemy.GlobalPosition = spawnGlobalPosition;
	}
}
