using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Turo.Ludo.Utilaĵoj;

public partial class Kuriero : BazMalamiko
{
	public abstract record AIStato
	{
		public record Ĉasi(bool Ŝargita) : AIStato;
	}

	public AIStato aiStato;
	
	public abstract record MovStato
	{
		public record MoviAl(Vector2I De, Vector2I Al, int KomenceTempo, int AlvenTempo, Func<float, float> Easing) : MovStato;
		public record Resti(Vector2I Loko, int FinTempo, bool Ŝarganta) : MovStato;
	}

	public MovStato movStato;

	public override void JeSurTeriĝo(Vector2I Loko, int alvenTempo)
	{
		base.JeKreiĝo();
		aiStato = new AIStato.Ĉasi(false);
		movStato = new MovStato.Resti(Loko,alvenTempo, false);
	}

	// farenda: havu du tipojn de movoj (atakoj kaj movoj)
	public override bool EstasVundiga() => movStato is MovStato.MoviAl;

	public record ĈasiLogiko(bool Atakus, Vector2I NunaLoko, Vector2I CelLoko);
	
	public ĈasiLogiko AkiriĈasiLogiko(Turo3d Turo, Tabulo tabulo, float TaktoDuro)
	{
		var turaTegolo = tabulo.TroviPlejProksimaTegolo(Turo.Position);
		var miaLoko = tabulo.TroviPlejProksimaTegolo(Position);
		var miaKoloro = tabulo.AkiriKoloroDeTegolo(miaLoko);
		var turRapido = Turo.Velocity;
		Vector2I ĝustigDirekto;
		int ĝustigKvanto;
		if (Mathf.Abs(turRapido.X) > Mathf.Abs(turRapido.Z))
		{
			ĝustigDirekto = Vector2I.Right * Mathf.Sign(turRapido.X);
			ĝustigKvanto = Mathf.RoundToInt(turRapido.X * TaktoDuro);
		}
		else
		{
			ĝustigDirekto = Vector2I.Right * Mathf.Sign(turRapido.Z);
			ĝustigKvanto = Mathf.RoundToInt(turRapido.Z * TaktoDuro);
		}

		Vector2I celTegolo = tabulo.KrampiEnTabulo(turaTegolo + ĝustigDirekto * ĝustigKvanto);
		if (tabulo.AkiriKoloroDeTegolo(celTegolo) != miaKoloro)
		{
			if (ĝustigDirekto == Vector2I.Zero || !tabulo.EstasEnTabulo(celTegolo + ĝustigDirekto))
			{
				var ebloj = new List<Vector2I>();
				if(celTegolo.X - 1 >= 0)
					ebloj.Add(new Vector2I(-1,0));
				if(celTegolo.Y - 1 >= 0)
					ebloj.Add(new Vector2I(0,-1));
				if(celTegolo.X + 1 < tabulo.Larĝo)
					ebloj.Add(new Vector2I(1,0));
				if(celTegolo.Y + 1 < tabulo.Alto)
					ebloj.Add(new Vector2I(0,1));
				if (ebloj.Count > 0)
				{
					ĝustigDirekto = ebloj[GD.RandRange(0, ebloj.Count - 1)];
				}
				else
				{
					ĝustigDirekto = Vector2I.Zero;
				}
			}
			celTegolo += ĝustigDirekto;
		}

		var dirs = new[]
		{
			new Vector2I(-1, -1),
			new Vector2I(-1, 1),
			new Vector2I(1, -1),
			new Vector2I(1, 1),
		};
		var orderedDirs = dirs.OrderBy(_ => -(_).AsFloat().Normalized().Dot((celTegolo - miaLoko).AsFloat().Normalized())).ToArray();
		var misAngulo = Mathf.Abs(orderedDirs[0].AsFloat().AngleTo((celTegolo - miaLoko).AsFloat()) % Mathf.Pi);
		var dif = celTegolo - miaLoko;
		if (Mathf.Abs(dif.X) == Mathf.Abs(dif.Y))
		{
			if (tabulo.AkiriKoloroDeTegolo(miaLoko) != tabulo.AkiriKoloroDeTegolo(celTegolo))
			{
				GD.PrintErr("Ŝanĝo de koloro");
			}
			if (!tabulo.EstasEnTabulo(celTegolo))
			{
				GD.PrintErr("Ekstertabula celo");
			}
			// Ataki
			return new ĈasiLogiko(true,miaLoko,celTegolo);
		}
		else
		{
			// farenda: sencimigu la eblon ŝanĝi kolorojn
			Vector2I flankaDir = orderedDirs[1];
			var flankaTegolo = (miaLoko +
									  (celTegolo - miaLoko).AsFloat()
										  .Project((flankaDir).AsFloat().Normalized())).RoundToInt();

			if (!tabulo.EstasEnTabulo(flankaTegolo))
			{
				var novaFlankaDir = orderedDirs[0];
				var novaFlankaTegolo = (miaLoko +
									  (celTegolo - miaLoko).AsFloat()
										  .Project((novaFlankaDir).AsFloat().Normalized())).RoundToInt();
				if (!tabulo.EstasEnTabulo(novaFlankaTegolo))
				{
					GD.PrintErr("Ekstertabula celo");
				}

				if (tabulo.AkiriKoloroDeTegolo(miaLoko) != tabulo.AkiriKoloroDeTegolo(novaFlankaTegolo))
				{
					GD.PrintErr("Ŝanĝo de koloro");
				}

				return new ĈasiLogiko(
					Atakus:false,
					NunaLoko:miaLoko,
					CelLoko:novaFlankaTegolo
				);
			}
			else
			{
				if (!tabulo.EstasEnTabulo(flankaTegolo))
				{
					GD.PrintErr("Ekstertabula celo");
				}
				if (tabulo.AkiriKoloroDeTegolo(miaLoko) != tabulo.AkiriKoloroDeTegolo(flankaTegolo))
				{
					GD.PrintErr("Ŝanĝo de koloro");
				}
				return new ĈasiLogiko(
					Atakus:false,
					NunaLoko:miaLoko,
					CelLoko:flankaTegolo
				);
			}
		}
	}

	public static List<Vector2I> OkupitajTegolojDePado(Vector2I De, Vector2I Al)
	{
		Vector2I Dif = Al - De;
		if (Math.Abs(Dif.X) != Math.Abs(Dif.Y))
		{
			GD.PrintErr("Nevalida Movo");
		}
		List<Vector2I> Tegoloj = new List<Vector2I>();
		Vector2I Dir = new Vector2I(Mathf.Sign(Dif.X), Mathf.Sign(Dif.Y));
		for (int i = 0; i <= Math.Abs(Dif.X); i++)
		{
			Tegoloj.Add(De + Dir * i);
		}

		return Tegoloj;
	}
}
