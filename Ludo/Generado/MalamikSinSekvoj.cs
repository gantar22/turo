using Godot;
using System;
using System.Linq;

public partial class MalamikSinSekvoj : Node
{
	public GenerGrupo[] Grupoj;

	public override void _Ready()
	{
		Grupoj = GetChildren().Select(ido => ido is GenerGrupo g ? g : null).Where(_ => _ != null).ToArray();
	}
}
