using Godot;
using System;

public partial class Venkis : Control
{
	[Signal]
	public delegate void JeKlakoEventHandler();

	public void Premi()
	{
		EmitSignal(SignalName.JeKlako);
		Hide();
	}
}
