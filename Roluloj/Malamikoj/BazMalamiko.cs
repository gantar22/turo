using Godot;
using System;



[GlobalClass]
public abstract partial class BazMalamiko : CharacterBody3D
{
	[Export] private Area3D vundigSkatolo;

	public virtual void JeKreiĝo()
	{
	}

	public virtual void JeSurTeriĝo()
	{
		if (MesaĝBuso.Singleton != null)
		{
			vundigSkatolo.BodyEntered += OnBodyEnter;
		}
	}

	void OnBodyEnter(Node3D node3D)
	{
		if (node3D is Turo3d)
		{
			if (MesaĝBuso.Singleton != null)
			{
				MesaĝBuso.Singleton.EmitSignal(MesaĝBuso.SignalName.JeTuroFrapita,this);
			}
		}
	}

	public abstract bool EstasVundiga();
}
