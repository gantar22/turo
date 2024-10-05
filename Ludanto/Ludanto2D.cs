using Godot;

namespace Ludanto;

public partial class Ludanto : CharacterBody2D
{
	public abstract class Stato
	{
		public class Natura : Stato
		{
			public Natura() { }
		}

		public class Vundata : Stato
		{
			public Vector2 retroPuŝDirekto;
			public float daŭro;
			public Stato revenStato;

			public Vundata(Vector2 retroPuŝDirekto, float daŭro, Stato revenStato)
			{
				this.retroPuŝDirekto = retroPuŝDirekto;
				this.daŭro = daŭro;
				this.revenStato = revenStato;
			}
		}

		public class Atakanta : Stato
		{
		}
	}

	public Stato stato = new Stato.Natura();
	public MovEnigo movEnigoj = new MovEnigo();

	[Export] private float bazaRapido = 250f;
	[Export] private float vundPuŝRapido = 350f;
	[Export] private float vundDaŭro = .15f;
	public int energio;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// get input
		Vector2 EnigDir = movEnigoj.AkiriEnigo()?.Direkto ?? Vector2.Zero;
		
		// tick
		switch (stato)
		{
			case Stato.Natura natura:
				Velocity = EnigDir * bazaRapido;
				MoveAndSlide();
				break;
			case Stato.Vundata vundata:
				vundata.daŭro -= (float)delta;
				if (vundata.daŭro < 0)
				{
					stato = vundata.revenStato;
				}
				else
				{
					Velocity = vundata.retroPuŝDirekto;
					MoveAndSlide();
				}
				break;
			case Stato.Atakanta atakanta:
				break;
		}
	}

	public void OnAreaEntered(Area2D alia)
	{
		if (alia is KurieroVundSkatolo)
		{
			switch (stato)
			{
				default:
					stato = new Stato.Vundata(
						(Position - alia.Position).Normalized() * vundPuŝRapido,
						vundDaŭro,
						stato
					);
					break;
			}
		}
		
	}
}