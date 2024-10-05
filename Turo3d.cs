using Godot;
using System;
using System.Linq;

public partial class Turo3d : CharacterBody3D
{
	public float Acelo = 1f;
	public const float BazRapido = 5.0f;
	public const float ĜustigaRapido = 20.0f;

	public abstract record Stato
	{
		public record Baza() : Stato;
		public record Vundata(Vector2 retroPuŝDirekto, float daŭro, Stato revenStato) : Stato;
		public record Atakanta() : Stato;
	}

	public Stato stato = new Stato.Baza();
	
	public override void _PhysicsProcess(double delta)
	{
		switch (stato)
		{
			case Stato.Atakanta atakanta:
				break;
			case Stato.Baza baza:
				_TraktiBazan(baza, delta);
				break;
			case Stato.Vundata vundata:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(stato));
		}
	}

	void _TraktiBazan(Stato.Baza stato, double delta)
	{
		Vector3 velocity = Velocity;

		Vector2 enigDirekto = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector2[] enigoDirektoj = new[] { Vector2.Down, Vector2.Up, Vector2.Right, Vector2.Left, Vector2.Zero };
		// farenda: trakti egalvenkojn
		enigDirekto = enigoDirektoj.MinBy(_ => _.DistanceSquaredTo(enigDirekto));
		Vector3 direction = (new Vector3(enigDirekto.X, 0, enigDirekto.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * BazRapido;
			velocity.Z = direction.Z * BazRapido;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, BazRapido);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, BazRapido);
		}

		// farenda: anstaŭ sekvi la plej najbaran kadro-linion, iri al la venonta laŭ nia rapid-direkto 
		float Ĝustigi(float f)
		{
			float Closestf = Mathf.Round(f);
			
			float signo = Mathf.Sign(Closestf - f);
			return signo * ĜustigaRapido * Mathf.Min(1,Mathf.Abs(Closestf - f));
		}
		if (velocity.X == 0f)
		{
			velocity.X += Ĝustigi(Position.X);
		}
		if (velocity.Z == 0f)
		{
			velocity.Z += Ĝustigi(Position.Z);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

}
