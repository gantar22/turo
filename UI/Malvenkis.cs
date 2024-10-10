using Godot;
using System;

public partial class Malvenkis : Control
{
	[Signal]
	public delegate void JeKlakoEventHandler();

	public void Premi()
	{
		EmitSignal(SignalName.JeKlako);
		Hide();
	}
}
