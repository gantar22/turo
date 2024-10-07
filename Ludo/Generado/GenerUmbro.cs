using Godot;
using System;

public partial class GenerUmbro : Node3D
{
	public void FiksiAlfon(float Alfo)
	{
		Scale = new Vector3(Alfo,1,Alfo);
	}
}
