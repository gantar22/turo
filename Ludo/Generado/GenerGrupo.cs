using Godot;
using System;
using System.Linq;

public partial class GenerGrupo : Node
{
	public GeneroDatumo[] GenerDatumoj;
	
	
	public override void _Ready()
	{
		GenerDatumoj = GetChildren().Select(ido => ido is GeneroDatumo g ? g : null).Where(_ => _ != null).ToArray();
	}
}
