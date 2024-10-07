using Godot;
using System;

public partial class MesaĝBuso : Node
{
	public static MesaĝBuso Singleton;

	[Signal]
	public delegate void JeTuroFrapitaEventHandler(BazMalamiko malamiko);

	public override void _Ready()
	{
		if (Singleton == null)
		{
			Singleton = this;
		}
	}
}
