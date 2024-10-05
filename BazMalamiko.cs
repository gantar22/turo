using Godot;
using System;

[GlobalClass]
public partial class BazMalamiko : CharacterBody3D
{
	[Export] private Area3D vundigSkatolo;


	public override void _Ready()
	{
		if(MesaĝBuso.Singleton != null)
		{}
		vundigSkatolo.BodyEntered += OnBodyEnter;
	}

	void OnBodyEnter(Node3D node3D)
	{
		if (node3D is TuroVundSkatolo)
		{
			if (MesaĝBuso.Singleton != null)
			{
				MesaĝBuso.Singleton.EmitSignal(MesaĝBuso.SignalName.JeTuroFrapita);
			}
		}
	}
}
