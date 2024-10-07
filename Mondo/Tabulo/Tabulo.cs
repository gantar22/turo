using Godot;
using System;
using System.Linq;

public partial class Tabulo : Node3D
{
	private Horizontalo[] Horizontaloj;
	
	public override void _Ready()
	{
		Horizontaloj = GetChildren().Select(ido => ido is Horizontalo h ? h : null).Where(_ => _ != null).ToArray();
	}
	
	public Vector2 AkiriTegolanPoz(Vector2I loko)
	{
		Vector3 poz = Horizontaloj[loko.Y].Tegoloj[loko.X].GlobalPosition;
		return new Vector2(poz.X, poz.Z);
	}

	// farenda: simpligi
	public Vector2I TroviPlejProksimaTegolo(Vector3 poz)
	{
		var indexed = Horizontaloj
			.SelectMany((h, i) => 
				h.Tegoloj
					.Select((t, j) => (i: i, j: j, t: t)));
		var rezulto = indexed.MinBy(_ => 
				_.t.GlobalPosition.DistanceSquaredTo(poz));
		
		return new Vector2I(rezulto.j, rezulto.i);
	}
	
	public enum Koloro {nigra,blanka}

	public Koloro AkiriKoloroDeTegolo(Vector2I Poz)
	{
		return ((Poz.X % 2) ^ (Poz.Y % 2)) == 0 ? Koloro.blanka : Koloro.nigra;
	}

	public int Larĝo => Horizontaloj[0].Tegoloj.Length;
	public int Alto => Horizontaloj.Length;

	public Vector3 KrampiPozEnTabulo(Vector3 Poz)
	{
		// farenda: ripari tion
		return new Vector3(Mathf.Clamp(Poz.X,-.5f,Larĝo - .5f), Poz.Y, Mathf.Clamp(Poz.Z, -Alto + .5f, .5f));
	}

	public Vector2I KrampiEnTabulo(Vector2I Loko)
	{
		return new Vector2I(Math.Clamp(Loko.X, 0, Larĝo - 1), Math.Clamp(Loko.Y, 0, Alto - 1));
	}

	public bool EstasEnTabulo(Vector2I Loko) => Loko == KrampiEnTabulo(Loko);
}
