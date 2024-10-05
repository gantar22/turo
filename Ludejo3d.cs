using Godot;
using System;
using System.Collections.Generic;

public partial class Ludejo3d : Node3D
{
	public abstract record Stato
	{
		public record NeKomencita() : Stato;
		public record Ludanta(Turo3d Turo,List<BazMalamiko> VivMalamikoj, int GrupIndico, float GrupTempo) : Stato;
	}

	private Stato stato = new Stato.NeKomencita();
	
	[Export] private Node3D GenerAlteco;

	[Export] private PackedScene turoSceno;
	
	public override void _Ready()
	{
		var turo = (Turo3d)turoSceno.Instantiate();
		stato = new Stato.Ludanta(turo, new List<BazMalamiko>(), 0, 0f);
	}

	public override void _Process(double delta)
	{
	}
}
