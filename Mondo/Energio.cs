using Godot;
using System;

public partial class Energio : Node3D
{
	public struct Surteriĝanta
	{
		public float Tempo;
		public Vector2I Loko;
		public GenerUmbro Umbro;
		public Func<float, float> Easing;
	}

	public Surteriĝanta? surteriĝanta;
	public bool claimed = false;

	public void JeSurTeriĝo(Vector2I valueLoko, int tempoInt)
	{
		
	}

	void OnBodyEnter(Node3D node3D)
	{
		switch (node3D)
		{
			case Turo3d turo:
				if (MesaĝBuso.Singleton != null)
				{
					MesaĝBuso.Singleton.EmitSignal(MesaĝBuso.SignalName.JeEnergioAkirita,this);
				}
				break;
			case BazMalamiko malamiko:
				if (MesaĝBuso.Singleton != null)
				{
					MesaĝBuso.Singleton.EmitSignal(MesaĝBuso.SignalName.JeEnergioDetruita,this);
				}
				break;
		}
	}
}
