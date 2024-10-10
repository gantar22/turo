using Godot;
using System;

public partial class EnergiKvanto : Node3D
{
	public void FiksiTekston(string teksto)
	{
		((TextMesh)((MeshInstance3D)GetNode("teksto")).Mesh).Text = teksto;
	}
}
