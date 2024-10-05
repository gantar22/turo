using Godot;
using System;

public partial class Kuriero : BazMalamiko
{
	public abstract record AIStato
	{
		public record Ĉasi() : AIStato;

		public record Generiĝi : AIStato;
	}

	public abstract record MovStato
	{
		public record MoviAl((int h, int v) De, (int h, int v) Al, float T, Func<float,float> Easing);
		public record Resti((int h, int v) Loko);
	}
}
