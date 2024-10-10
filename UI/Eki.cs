using Godot;
using System;

public partial class Eki : Control
{
	[Signal]
	public delegate void JeKlakoEventHandler();

	public void Ek()
	{
		EmitSignal(SignalName.JeKlako);
		Hide();
	}
}
