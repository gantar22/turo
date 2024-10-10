using Godot;
using System;

public partial class MesaĝBuso : Node
{
	public static MesaĝBuso Singleton;

	[Signal]
	public delegate void JeTuroFrapitaEventHandler(BazMalamiko malamiko);
	[Signal]
	public delegate void JeEnergioAkiritaEventHandler(Energio energio);
	[Signal]
	public delegate void JeEnergioDetruitaEventHandler(Energio energio);

	public override void _Ready()
	{
		{
			Singleton = this;
		}
	}
}
