using Godot;
using System;
using System.Linq;

public partial class Horizontalo : Node3D
{
	public Tegolo[] Tegoloj;

	public override void _Ready()
	{
		Tegoloj = GetChildren().Select(ido => ido is Tegolo t ? t : null).Where(_ => _ != null).ToArray();
	}
}
