using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Turo.Ludo.Utilaĵoj;

public partial class Ludejo3d : Node3D
{
	public struct SurTeriĝDatumo
	{
		public float NormaligitaTempo;
		public Vector2I Loko;
		public GenerUmbro Umbro;
		public Func<float, float> Easing;
	}

	public enum TegoloOkupiĝKialo
	{
		Surteriĝo,
		Ĉesto,
		Plano,
	}

	public record TegoloOkupiĝDatumo(Vector2I Loko, BazMalamiko m, TegoloOkupiĝKialo kialo);

	public abstract record Stato
	{
		public record NeKomencita() : Stato;
		public record Ludanta(
			 Turo3d Turo
			,List<BazMalamiko> VivMalamikoj
			,List<(BazMalamiko m,SurTeriĝDatumo d)> SurTeriĝantoj
			,int GrupIndico
			,float? GrupTempo
			,List<TegoloOkupiĝDatumo> OkupitajTegoloj
			,int Vivoj
		) : Stato;
	}

	private Stato stato = new Stato.NeKomencita();
	
	[Export] private PackedScene turoSceno;
	[Export] private Tabulo tabulo;
	
	[Export,ExportGroup("Generado")] private Node3D GenerAlteco;

	[Export,ExportGroup("Generado")] private MalamikSinSekvoj malamikSinSekvoj;

	[Export,ExportGroup("Generado")] private PackedScene generUmbroSceno;

	[Export,ExportGroup("Malamikoj")] private PackedScene PeonoSceno;
	[Export,ExportGroup("Malamikoj")] private PackedScene KurieroSceno;
	[Export,ExportGroup("Malamikoj")] private PackedScene ĈevaloSceno;
	[Export,ExportGroup("Malamikoj")] private PackedScene DamoSceno;
	[Export,ExportGroup("Malamikoj")] private PackedScene ReĝoSceno;

	[Export, ExportGroup("UI")] private Node3D[] Koroj;

	private float TaktoDuro = 60f/120f;
	
	public override void _Ready()
	{
		var turo = (Turo3d)turoSceno.Instantiate();
		AddChild(turo);
		var turoPoz = tabulo.AkiriTegolanPoz(new Vector2I(2,2));
		turo.Position = new Vector3(turoPoz.X, 0, turoPoz.Y);
		stato = new Stato.Ludanta(
			turo,
			new List<BazMalamiko>(),
			new List<(BazMalamiko, SurTeriĝDatumo)>(),
			0,
			null,
			new List<TegoloOkupiĝDatumo>(),
			3
		);
		MesaĝBuso.Singleton.Connect(MesaĝBuso.SignalName.JeTuroFrapita, Callable.From((BazMalamiko _) => JeFrapiĝo(_)));
	}

	public override void _Process(double delta)
	{
		switch (stato)
		{
			case Stato.Ludanta ludanta:
				var GrupTempo = (ludanta.GrupTempo ?? 0) + (float)delta * 1f/TaktoDuro; // farenda: uzi la takto de la muziko
				var GrupTempoInt = Mathf.FloorToInt(GrupTempo);
				if (Mathf.FloorToInt(ludanta.GrupTempo ?? -1) != GrupTempoInt)
					{
						// Generi la sekvajn malamikojn
						GeneriMalamikojn(ludanta, GrupTempoInt);

						// regi malamikojn AIjn
						foreach (var malamiko in ludanta.VivMalamikoj)
						{
							switch (malamiko)
							{
								case Kuriero kuriero:
									// Pretersalti se la movStato ne estas finita
									switch (kuriero.movStato)
									{
										case Kuriero.MovStato.MoviAl moviAl:
											if (moviAl.AlvenTempo != GrupTempoInt)
												continue;
											break;
										case Kuriero.MovStato.Resti resti:
											if (resti.FinTempo != GrupTempoInt)
												continue;
											break;
										default:
											break;
									}

									switch (kuriero.aiStato)
									{
										case Kuriero.AIStato.Ĉasi ĉasi:
											var Logiko = kuriero.AkiriĈasiLogiko(ludanta.Turo, tabulo, TaktoDuro,ludanta.OkupitajTegoloj);
											if (Logiko.Atakus)
											{
												if (ĉasi.Ŝargita)
												{
													kuriero.movStato = new Kuriero.MovStato.MoviAl(Logiko.NunaLoko,
														Logiko.CelLoko, GrupTempoInt, GrupTempoInt + Mathf.Abs(Logiko.CelLoko.X - Logiko.NunaLoko.X), Easings.EaseInOut);
													kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita: false);
												}
												else
												{
													kuriero.movStato = new Kuriero.MovStato.Resti(Logiko.CelLoko, GrupTempoInt + 1, true);
													kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita:true);
												}
											}
											else
											{
												kuriero.movStato = new Kuriero.MovStato.MoviAl(Logiko.NunaLoko, Logiko.CelLoko, 
													GrupTempoInt, GrupTempoInt + Mathf.Abs(Logiko.CelLoko.X - Logiko.NunaLoko.X), Easings.EaseInOut);
												kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita:false);
											}
											break;
										default:
											throw new ArgumentOutOfRangeException();
									}

									break;
								default:
									throw new ArgumentOutOfRangeException(nameof(malamiko));
							}
						}
					}


				// regi surteĝantojn
				RegiSurTeriĝantojn(delta, ludanta);

				// Movi la figurojn
				foreach (var malamiko in ludanta.VivMalamikoj)
				{
					switch (malamiko)
					{
						case Kuriero kuriero:
							switch (kuriero.movStato)
							{
								case Kuriero.MovStato.MoviAl moviAl:
									if (moviAl.AlvenTempo == GrupTempoInt)
									{
										// ni devas electi novan AI decidon
										kuriero.movStato = new Kuriero.MovStato.Resti(moviAl.Al, GrupTempoInt+1,false);
									}
									else
									{
										var de = tabulo.AkiriTegolanPoz(moviAl.De);
										var al = tabulo.AkiriTegolanPoz(moviAl.Al);
										float T = (GrupTempo - moviAl.KomenceTempo) / (moviAl.AlvenTempo - moviAl.KomenceTempo);
										float Alfo = moviAl.Easing(T);
										Vector2 Loko = de.Lerp(al, Alfo);
										kuriero.Position = new Vector3(Loko.X, 0, Loko.Y);
									}
									break;
								case Kuriero.MovStato.Resti resti:
									break;
								default:
									break;
							}

							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(malamiko));
					}
				}

				ludanta.Turo.Ĝisdatigi(delta,tabulo);
				for (int i = 0; i < Koroj.Length; i++)
				{
					Koroj[i].Visible = i < ludanta.Vivoj;
				}
				if(false)
				{
					var ludantoLoko = tabulo.TroviPlejProksimaTegolo(ludanta.Turo.Position);
					GD.Print($"Tegola:    ({ludantoLoko.X},{ludantoLoko.Y})");
					GD.Print($"Globa:     ({ludanta.Turo.Position.X},{ludanta.Turo.Position.Z})");
					var TegolaPoz = tabulo.AkiriTegolanPoz(ludantoLoko);
					GD.Print($"TegolaPos: ({TegolaPoz.X},{TegolaPoz.Y})");
					GD.Print(tabulo.AkiriKoloroDeTegolo(ludantoLoko) == Tabulo.Koloro.blanka ? "blanka" : "nigra");
				}
				stato = ludanta with { GrupTempo=GrupTempo };
				break;
			case Stato.NeKomencita neKomencita:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(stato));
		}
	}

	private void RegiSurTeriĝantojn(double delta, Stato.Ludanta ludanta)
	{
		for (var indico = ludanta.SurTeriĝantoj.Count - 1; indico >= 0; indico--)
		{
			var (malamiko, datumo) = ludanta.SurTeriĝantoj[indico];
			bool finita = false;
			float T = datumo.NormaligitaTempo + (float)delta;
			if (T > 1)
			{
				T = 1f;
				finita = true;
			}

			datumo.NormaligitaTempo = T;

			Vector2 Celo = tabulo.AkiriTegolanPoz(datumo.Loko);
			float A = datumo.Easing(T);
			float Y = Mathf.Lerp(GenerAlteco.Position.Y, 0, A);
			malamiko.Position = new Vector3(Celo.X, Y, Celo.Y);
			datumo.Umbro.FiksiAlfon(A);

			if (finita)
			{
				datumo.Umbro.QueueFree();
				ludanta.SurTeriĝantoj.RemoveAt(indico);
				malamiko.JeSurTeriĝo();
				ludanta.VivMalamikoj.Add(malamiko);
			}
			else
			{
				ludanta.SurTeriĝantoj[indico] = (malamiko,datumo);
			}
		}
	}

	private void GeneriMalamikojn(Stato.Ludanta ludanta, int GrupTempoInt)
	{
		foreach(var Genero in malamikSinSekvoj.Grupoj[ludanta.GrupIndico].GenerDatumoj.Where(_=>_.RelativaGenerTempo == GrupTempoInt))
		{
			if (ludanta.OkupitajTegoloj.Any(_ => _.Loko.X == Genero.Horizontalo && _.Loko.Y == Genero.Vertikalo))
			{
				// prokrasti ĝis la loko estas ne-okupata
				Genero.RelativaGenerTempo++;
				continue;
			}
			PackedScene Generoto = Genero.Tipo switch
			{
				MalamikoTipoj.Peono => PeonoSceno,
				MalamikoTipoj.Kuriero => KurieroSceno,
				MalamikoTipoj.Ĉevalo => ĈevaloSceno,
				MalamikoTipoj.Damo => DamoSceno,
				MalamikoTipoj.Reĝo => ReĝoSceno,
				_ => throw new ArgumentOutOfRangeException()
			};
			BazMalamiko Generito = (BazMalamiko)Generoto.Instantiate();
			AddChild(Generito);
			Vector2 Celo = tabulo.AkiriTegolanPoz(new Vector2I(Genero.Horizontalo, Genero.Vertikalo));
			Generito.Position = new Vector3(Celo.X,GenerAlteco.Position.Y,Celo.Y);
			Generito.JeKreiĝo();
			var Umbro = (GenerUmbro)generUmbroSceno.Instantiate();
			AddChild(Umbro);
			Umbro.Position = new Vector3(Celo.X,0, Celo.Y);
			ludanta.OkupitajTegoloj.Add(new TegoloOkupiĝDatumo(new Vector2I(Genero.Horizontalo,Genero.Vertikalo),Generito,TegoloOkupiĝKialo.Surteriĝo));
			ludanta.SurTeriĝantoj.Add((Generito, new SurTeriĝDatumo()
			{
				Loko = new Vector2I(Genero.Horizontalo,Genero.Vertikalo),
				Easing = a => 1f - ((1-a) * (1-a)),
				NormaligitaTempo = 0f,
				Umbro = Umbro,
			}));
		}
	}

	private void JeFrapiĝo(BazMalamiko malamiko)
	{
		if (stato is Stato.Ludanta ludanta)
		{
			if (malamiko.EstasVundiga())
			{
				ludanta.Turo.stato = new Turo3d.Stato.Mortinta();
				ludanta.Turo.Hide();
				// farenda: kreu gibletojn
				stato = ludanta with { Vivoj = ludanta.Vivoj - 1 };
				if (ludanta.Vivoj < 0)
				{
					//farenda: GAME OVER
				}
			}
		}
	}
}
