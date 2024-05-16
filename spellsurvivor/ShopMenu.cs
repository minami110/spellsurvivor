using Godot;
using System;
using R3;

namespace spellsurvivor;

public partial class ShopMenu : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		
		Main.GameMode.WaveEnded.Subscribe(_ =>
		{
			OpenShop();
		});
	}
	
	private void OpenShop()
	{
		Show();
	}

	public void CloseShop()
	{
		Hide();
		Main.GameMode.ExitShop();
	}
}
