using Godot;
using System;
using System.Linq;

public partial class Turo3d : CharacterBody3D
{
	[Export] private Node3D[] DiafanajKopioj;
	public const float BazRapido = 5.0f;
	public const float ĜustigaRapido = 20.0f;
	public const float FulmoDaŭro = .05f;
	public const float RefortiĝDaŭro = .075f;

	public abstract record Stato
	{
		public record Baza() : Stato;
		public record Komenca(Vector2I Loko, float Tempo) : Stato;
		public record Vundata(Vector2I Destino, float daŭro) : Stato;
		public record Atakanta(Vector3 De, Vector3 Al, int Kvanto, float Tempo, int etapo) : Stato;

		public record Mortinta(float Tempo) : Stato;
	}

	public Stato stato = new Stato.Baza();
	public int energiKvanto = 0;
	
	public void Ĝisdatigi(double delta, Tabulo tabulo)
	{
		switch (stato)
		{
			case Stato.Atakanta atakanta:
				TraktiAtakantan(atakanta,delta,tabulo);
				break;
			case Stato.Baza baza:
				TraktiBazan(baza, delta, tabulo);
				break;
			case Stato.Vundata vundata:
				break;
			case Stato.Mortinta mortinta:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(stato));
		}
	}

	public void TraktiAtakantan(Stato.Atakanta atakanta, double delta, Tabulo tabulo)
	{
		float Tempo = atakanta.Tempo + (float)delta;
		// "daŝi"
		if (atakanta.etapo < atakanta.Kvanto)
		{
			Tempo = Mathf.Min(Tempo,FulmoDaŭro);
			Vector3 celo = atakanta.De.Lerp(atakanta.Al, (atakanta.etapo + atakanta.Tempo / FulmoDaŭro) / (float)atakanta.Kvanto);
			Velocity = (celo - Position) / (float)delta;
			MoveAndSlide();
			
			// pro la min, ĉi-tiu egalkontrolo licas
			if (Tempo == FulmoDaŭro)
			{
				// farenda: montru diafanan kopion
				stato = atakanta with { etapo = atakanta.etapo + 1, Tempo = 0};
				return;
			}

			stato = atakanta with { Tempo = Tempo };
			return;
		}
		
		// refortiĝi
		if (Tempo > RefortiĝDaŭro)
		{
			stato = new Stato.Baza();
			return;
		}

		stato = atakanta with { Tempo = Tempo };
	}


	public void TraktiBazan(Stato.Baza baza, double delta, Tabulo tabulo)
	{
		Vector3 velocity = Velocity;

		Vector2 enigDirekto = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector2[] enigoDirektoj = new[] { Vector2.Down, Vector2.Up, Vector2.Right, Vector2.Left, Vector2.Zero };
		// farenda: trakti egalvenkojn
		enigDirekto = enigoDirektoj.MinBy(_ => _.DistanceSquaredTo(enigDirekto));
		Vector3 direction = (new Vector3(enigDirekto.X, 0, enigDirekto.Y)).Normalized();
		
		if (Input.IsActionJustPressed("primary"))
		{
			if (energiKvanto > 0)
			{
				stato = KomenciAtakon(tabulo,direction, ref energiKvanto);
			}
			return;
		}
		
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
		Vector3 Ĝustigi(Vector3 f)
		{
			Vector2 Closestf = tabulo.AkiriTegolanPoz(tabulo.TroviPlejProksimaTegolo(f));
			Vector3 Closestf3 = new Vector3(Closestf.X, 0, Closestf.Y);
			return (Closestf3 - f).Normalized() * ĜustigaRapido * Mathf.Min(1, (Closestf3 - f).Length());
		}
		if (velocity.X == 0f)
		{
			velocity.X += Ĝustigi(Position).X;
		}
		if (velocity.Z == 0f)
		{
			velocity.Z += Ĝustigi(Position).Z;
		}

		var krampitaRapido = (tabulo.KrampiPozEnTabulo(velocity * (float)delta + Position) - Position) / (float)delta;

		Velocity = krampitaRapido;
		MoveAndSlide();
	}

	Stato KomenciAtakon(Tabulo tabulo, Vector3 vel, ref int energio)
	{
		Vector3 De = Position;
		Vector3 Al = De + vel * energio;
		Al = tabulo.KrampiPozEnTabulo(Al);
		var uzitaEnergio = Mathf.RoundToInt((Al - De).Floor().Length());
		energio -= uzitaEnergio;
		
		return new Stato.Atakanta(De,Al,uzitaEnergio,0f,0);
	}
	
}
