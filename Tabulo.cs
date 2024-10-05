using Godot;
using System;
using System.Linq;

public partial class Tabulo : Node3D
{
	[Export] private Horizontalo[] Horizontaloj;

	public Vector2 AkiriTegolaPoz(int hor, int ver)
	{
		Vector3 poz = Horizontaloj[hor].Tegoloj[ver].Position;
		return new Vector2(poz.X, poz.Z);
	}

	// farenda: simpligi
	public (int h, int v) TroviPlejProksimaTegolo(Vector3 poz)
	{
		var rezulto = Horizontaloj
			.SelectMany((h, i) => 
				h.Tegoloj
					.Select((t, j) => (i: i, j: j, t: t)))
			.MinBy(_ => 
				_.t.Position.DistanceSquaredTo(poz));
		
		return (rezulto.i, rezulto.j);
	}
}
