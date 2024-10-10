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

	public record TegoloOkupiĝDatumo(BazMalamiko m, TegoloOkupiĝKialo kialo);

	public abstract record Stato
	{
		public record NeKomencita() : Stato;
		public record Ludanta(
			 Turo3d Turo
			,List<BazMalamiko> VivMalamikoj
			,List<(BazMalamiko m,SurTeriĝDatumo d)> SurTeriĝantoj
			,int GrupIndico
			,float? GrupTempo
			,int Vivoj
			,List<Energio> Energioj
		) : Stato;
	}

	private Stato stato = new Stato.NeKomencita();
	
	[Export] private PackedScene turoSceno;
	[Export] private PackedScene energioSceno;
	[Export] private Tabulo tabulo;

	[Export] private Eki eki;
	[Export] private Venkis venkis;
	[Export] private Malvenkis malvenkis;

	[Export] private EnergiKvanto energiKvanto;
	
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
	private int   TaktoGrupo = 3;
	private float RegenerDuro = 3f;
	
	public override void _Ready()
	{
		var turo = (Turo3d)turoSceno.Instantiate();
		AddChild(turo);
		var turoPoz = tabulo.AkiriTegolanPoz(new Vector2I(2,2));
		turo.Position = new Vector3(turoPoz.X, 0, turoPoz.Y);
		stato = new Stato.NeKomencita();
		MesaĝBuso.Singleton.Connect(MesaĝBuso.SignalName.JeTuroFrapita, Callable.From((BazMalamiko _) => JeFrapiĝo(_)));
		MesaĝBuso.Singleton.Connect(MesaĝBuso.SignalName.JeEnergioAkirita, Callable.From((Energio _) => AkiriEnergion(_)));
		MesaĝBuso.Singleton.Connect(MesaĝBuso.SignalName.JeEnergioDetruita, Callable.From((Energio _) => DetruiEnergion(_)));
		eki.Connect(Eki.SignalName.JeKlako,Callable.From(() =>
		{
			stato = new Stato.Ludanta(
				turo,
				new List<BazMalamiko>(),
				new List<(BazMalamiko, SurTeriĝDatumo)>(),
				0,
				null,
				3,
				new List<Energio>()
			);
		}));
		venkis.Connect(Venkis.SignalName.JeKlako, Callable.From(() =>
		{
			
			venkis.Hide();
			QueueFree();
			GetTree().Root.AddChild(ResourceLoader.Load<PackedScene>("res://Ludo/ludejo_3d.tscn").Instantiate());
		}));
		malvenkis.Connect(Malvenkis.SignalName.JeKlako, Callable.From(() =>
		{
			malvenkis.Hide();
			QueueFree();
			GetTree().Root.AddChild(ResourceLoader.Load<PackedScene>("res://Ludo/ludejo_3d.tscn").Instantiate());
		}));
	}

	public override void _Process(double delta)
	{
		switch (stato)
		{
			case Stato.Ludanta ludanta:
				Dictionary<Vector2I, TegoloOkupiĝDatumo> OkupitajTegoloj =
					new Dictionary<Vector2I, TegoloOkupiĝDatumo>();
				TroviOkupatajnTegolojn(ludanta, OkupitajTegoloj);

				var GrupTempo =
					(ludanta.GrupTempo ?? 0) + (float)delta * 1f / TaktoDuro; // farenda: uzi la takto de la muziko
				var GrupTempoInt = Mathf.FloorToInt(GrupTempo);
				bool ludantoEstasLibera = ludanta.Turo.stato is not Turo3d.Stato.Mortinta &&
				                          ludanta.Turo.stato is not Turo3d.Stato.Komenca;
				// Fari "je-takta"jn aferojn
				if (Mathf.FloorToInt(ludanta.GrupTempo ?? -1) != GrupTempoInt && ludantoEstasLibera)
				{
					// Generi la sekvajn malamikojn
					GeneriMalamikojn(ludanta, GrupTempoInt, OkupitajTegoloj);
					{
						if(ludanta.GrupIndico < malamikSinSekvoj.Grupoj.Length && malamikSinSekvoj.Grupoj[ludanta.GrupIndico].GenerDatumoj.All(_ => _.RelativaGenerTempo < GrupTempoInt))
						{
							if (!ludanta.SurTeriĝantoj.Any() && !ludanta.VivMalamikoj.Any())
							{
								stato = ludanta with { GrupTempo = null, GrupIndico = ludanta.GrupIndico + 1};
								return;
							}
						}

						if (ludanta.GrupIndico >= malamikSinSekvoj.Grupoj.Length)
						{
							venkis.Show();
						}
					}

					// Generi plibonigojn
					GeneriPlibonigojn(ludanta, GrupTempoInt, OkupitajTegoloj);

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
										if (moviAl.AlvenTempo > GrupTempoInt)
											continue;
										break;
									case Kuriero.MovStato.Resti resti:
										if (resti.FinTempo > GrupTempoInt)
											continue;
										break;
									default:
										break;
								}

								switch (kuriero.aiStato)
								{
									case Kuriero.AIStato.Ĉasi ĉasi:
										var Logiko = kuriero.AkiriĈasiLogiko(ludanta.Turo, tabulo, TaktoDuro);
										if (Kuriero.OkupitajTegolojDePado(Logiko.NunaLoko, Logiko.CelLoko).Any(_ =>
											    OkupitajTegoloj.TryGetValue(_, out var d) && d.m != kuriero))
										{
											if (OkupitajTegoloj.TryGetValue(Logiko.NunaLoko, out var d) &&
											    d.m != kuriero)
											{
												GD.PrintErr("neokupata tegolo kiu estas okupata");
											}

											if (!OkupitajTegoloj.ContainsKey(Logiko.NunaLoko))
											{
												OkupitajTegoloj.Add(Logiko.NunaLoko,
													new TegoloOkupiĝDatumo(kuriero, TegoloOkupiĝKialo.Ĉesto));
											}

											kuriero.movStato = new Kuriero.MovStato.Resti(Logiko.NunaLoko,
												GrupTempoInt + 1, false);
											kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita: ĉasi.Ŝargita);
											TroviOkupatajnTegolojn(ludanta,
												new Dictionary<Vector2I, TegoloOkupiĝDatumo>());
										}
										else
										{
											if (Logiko.Atakus)
											{
												if (ĉasi.Ŝargita)
												{
													foreach (var tegolo in Kuriero.OkupitajTegolojDePado(
														         Logiko.NunaLoko, Logiko.CelLoko))
													{
														if (!(OkupitajTegoloj.TryGetValue(tegolo, out var d) &&
														      d.m == kuriero))
														{
															OkupitajTegoloj.Add(tegolo,
																new TegoloOkupiĝDatumo(kuriero,
																	TegoloOkupiĝKialo.Plano));
														}
													}

													kuriero.movStato = new Kuriero.MovStato.MoviAl(Logiko.NunaLoko,
														Logiko.CelLoko, GrupTempoInt,
														GrupTempoInt + Mathf.Abs(Logiko.CelLoko.X - Logiko.NunaLoko.X),
														Easings.EaseInOut);
													kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita: false);
													TroviOkupatajnTegolojn(ludanta,
														new Dictionary<Vector2I, TegoloOkupiĝDatumo>());
												}
												else
												{
													if (OkupitajTegoloj.TryGetValue(Logiko.NunaLoko, out var d) &&
													    d.m != kuriero)
													{
														GD.PrintErr("neokupata tegolo kiu estas okupata");
													}

													if (!OkupitajTegoloj.ContainsKey(Logiko.NunaLoko))
													{
														OkupitajTegoloj.Add(Logiko.NunaLoko,
															new TegoloOkupiĝDatumo(kuriero, TegoloOkupiĝKialo.Ĉesto));
													}

													kuriero.movStato = new Kuriero.MovStato.Resti(Logiko.NunaLoko,
														GrupTempoInt + 1, true);
													kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita: true);
													TroviOkupatajnTegolojn(ludanta,
														new Dictionary<Vector2I, TegoloOkupiĝDatumo>());
												}
											}
											else
											{
												foreach (var tegolo in Kuriero.OkupitajTegolojDePado(Logiko.NunaLoko,
													         Logiko.CelLoko))
												{
													if (!(OkupitajTegoloj.TryGetValue(tegolo, out var d) &&
													      d.m == kuriero))
													{
														OkupitajTegoloj.Add(tegolo,
															new TegoloOkupiĝDatumo(kuriero, TegoloOkupiĝKialo.Plano));
													}
												}

												kuriero.movStato = new Kuriero.MovStato.MoviAl(Logiko.NunaLoko,
													Logiko.CelLoko,
													GrupTempoInt,
													GrupTempoInt + Mathf.Abs(Logiko.CelLoko.X - Logiko.NunaLoko.X),
													Easings.EaseInOut);
												kuriero.aiStato = new Kuriero.AIStato.Ĉasi(Ŝargita: false);
												TroviOkupatajnTegolojn(ludanta,
													new Dictionary<Vector2I, TegoloOkupiĝDatumo>());
											}
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
				TraktiSurTeriĝantojn(delta, ludanta, GrupTempoInt);

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
										kuriero.movStato =
											new Kuriero.MovStato.Resti(moviAl.Al, GrupTempoInt + 1, false);
									}
									else
									{
										var de = tabulo.AkiriTegolanPoz(moviAl.De);
										var al = tabulo.AkiriTegolanPoz(moviAl.Al);
										float T = (GrupTempo - moviAl.KomenceTempo) /
										          (moviAl.AlvenTempo - moviAl.KomenceTempo);
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

				switch (ludanta.Turo.stato)
				{
					case Turo3d.Stato.Komenca komenca:
						Vector2 Celo = tabulo.AkiriTegolanPoz(komenca.Loko);
						float A = komenca.Tempo + (float)delta;
						if (A < 1)
						{
							float Y = Mathf.Lerp(GenerAlteco.Position.Y, 0, A);
							ludanta.Turo.Position = new Vector3(Celo.X, Y, Celo.Y);
							ludanta.Turo.stato = komenca with { Tempo = A };
						}
						else
						{
							ludanta.Turo.stato = new Turo3d.Stato.Baza();
						}
						break;
					case Turo3d.Stato.Atakanta atakanta:
						ludanta.Turo.TraktiAtakantan(atakanta, delta, tabulo);
						break;
					case Turo3d.Stato.Baza baza:
						ludanta.Turo.TraktiBazan(baza, delta, tabulo);
						break;
					case Turo3d.Stato.Vundata vundata:
						break;
					case Turo3d.Stato.Mortinta mortinta:
						float NovaTempo = mortinta.Tempo - (float)delta;
						if (NovaTempo < 0)
						{
							Vector2I? Loko = null;
							// try to spawn some spots
							for (int i = 0; i < 32; i++)
							{
								var provLoko = new Vector2I(GD.RandRange(0, tabulo.Larĝo - 1), GD.RandRange(0, tabulo.Alto - 1));
								if (!OkupitajTegoloj.ContainsKey(provLoko))
								{
									Loko = provLoko;
									break;
								}
							}

							// todo mortigu malamikon se tio necesas
							if (Loko.HasValue)
							{
								ludanta.Turo.stato = new Turo3d.Stato.Komenca(Loko.Value, 0);
								ludanta.Turo.Position = GenerAlteco.Position;
								ludanta.Turo.Show();
							}
						}
						else
						{
							ludanta.Turo.stato = mortinta with { Tempo = NovaTempo };
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(stato));
				}

				energiKvanto.FiksiTekston(ludanta.Turo.energiKvanto.ToString());
				for (int i = 0; i < Koroj.Length; i++)
				{
					Koroj[i].Visible = i < ludanta.Vivoj;
				}

				if (ludanta.Vivoj < 0)
				{
					malvenkis.Show();
				}

				stato = ludanta with { GrupTempo = GrupTempo };
				break;
			case Stato.NeKomencita neKomencita:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(stato));
		}
	}

	private void GeneriPlibonigojn(Stato.Ludanta ludanta, int grupTempoInt, Dictionary<Vector2I,TegoloOkupiĝDatumo> okupitajTegoloj)
	{
		if (ludanta.Turo.stato is Turo3d.Stato.Mortinta)
			return;
		if (grupTempoInt % TaktoGrupo == 0)
		{
			if (GD.Randf() < 1f/2f)
			{
				Vector2I? Loko = null;
				// try to spawn some spots
				for (int i = 0; i < 32; i++)
				{
					var provLoko = new Vector2I(GD.RandRange(0, tabulo.Larĝo - 1), GD.RandRange(0, tabulo.Alto - 1));
					if (!okupitajTegoloj.ContainsKey(provLoko))
					{
						Loko = provLoko;
						break;
					}
				}

				if (!Loko.HasValue)
				{
					// we couldn't find a spot
					return;
				}
				Energio Energio = (Energio)energioSceno.Instantiate();
				var Poz = tabulo.AkiriTegolanPoz(Loko.Value);
				Energio.Position = new Vector3(Poz.X, GenerAlteco.Position.Y, Poz.Y);
				var umbro = (GenerUmbro)generUmbroSceno.Instantiate();
				umbro.Position = new Vector3(Poz.X, 0f, Poz.Y);
				AddChild(umbro);
				
				Energio.surteriĝanta = new Energio.Surteriĝanta
				{
					Tempo = grupTempoInt,
					Loko = Loko.Value,
					Umbro = umbro,
					Easing = x=>x
				};
				AddChild(Energio);
				ludanta.Energioj.Add(Energio);
			}
		}
	}

	private static void TroviOkupatajnTegolojn(Stato.Ludanta ludanta, Dictionary<Vector2I, TegoloOkupiĝDatumo> outOkupitajTegoloj)
	{
		foreach (var (m,d) in ludanta.SurTeriĝantoj)
		{
			outOkupitajTegoloj.Add(d.Loko,new TegoloOkupiĝDatumo(m,TegoloOkupiĝKialo.Surteriĝo));
		}

		foreach (var malamiko in ludanta.VivMalamikoj)
		{
			switch (malamiko)
			{
				case Kuriero kuriero:
					switch (kuriero.movStato)
					{
						case Kuriero.MovStato.MoviAl moviAl:
							// farenda: faru tion pli precisa
							Vector2I nunaLoko = moviAl.De;
							foreach(var tegolo in Kuriero.OkupitajTegolojDePado(nunaLoko, moviAl.Al))
							{
								// farenda ĉu tio senshavas?
								outOkupitajTegoloj.Add(tegolo,new TegoloOkupiĝDatumo(kuriero,TegoloOkupiĝKialo.Plano));
							}
							break;
						case Kuriero.MovStato.Resti resti:
							outOkupitajTegoloj.Add(resti.Loko,new TegoloOkupiĝDatumo(kuriero,TegoloOkupiĝKialo.Ĉesto));
							break;
						default:
							break;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(malamiko));
			}
		}
	}


	private void TraktiSurTeriĝantojn(double delta, Stato.Ludanta ludanta, int TempoInt)
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
				malamiko.JeSurTeriĝo(datumo.Loko,TempoInt);
				ludanta.VivMalamikoj.Add(malamiko);
			}
			else
			{
				ludanta.SurTeriĝantoj[indico] = (malamiko,datumo);
			}
		}
		for (var indico = ludanta.Energioj.Count - 1; indico >= 0; indico--)
		{
			var energio = ludanta.Energioj[indico];
			bool finita = false;
			if (!energio.surteriĝanta.HasValue)
			{
				continue;
			}
			float T = energio.surteriĝanta.Value.Tempo + (float)delta;
			if (T > 1)
			{
				T = 1f;
				finita = true;
			}

			energio.surteriĝanta = energio.surteriĝanta.Value with { Tempo = T };

			Vector2 Celo = tabulo.AkiriTegolanPoz(energio.surteriĝanta.Value.Loko);
			float A = energio.surteriĝanta.Value.Easing(T);
			float Y = Mathf.Lerp(GenerAlteco.Position.Y, 0, A);
			energio.Position = new Vector3(Celo.X, Y, Celo.Y);
			energio.surteriĝanta.Value.Umbro.FiksiAlfon(A);

			if (finita)
			{
				energio.surteriĝanta.Value.Umbro.QueueFree();
				energio.surteriĝanta = null;
			}
		}
	}

	private void GeneriMalamikojn(Stato.Ludanta ludanta, int GrupTempoInt, Dictionary<Vector2I,TegoloOkupiĝDatumo> outOkupiĝDatumo)
	{
		if (malamikSinSekvoj.Grupoj.Length <= ludanta.GrupIndico)
		{
			return;
		}
		foreach(var Genero in malamikSinSekvoj.Grupoj[ludanta.GrupIndico].GenerDatumoj.Where(_=>_.RelativaGenerTempo == GrupTempoInt))
		{
			if (outOkupiĝDatumo.Any(_ => _.Key.X == Genero.Horizontalo && _.Key.Y == Genero.Vertikalo))
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
			outOkupiĝDatumo.Add(new Vector2I(Genero.Horizontalo,Genero.Vertikalo), new TegoloOkupiĝDatumo(Generito,TegoloOkupiĝKialo.Surteriĝo));
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
			switch (ludanta.Turo.stato)
			{
				case Turo3d.Stato.Atakanta atakanta:
					malamiko.QueueFree();
					ludanta.VivMalamikoj.Remove(malamiko);
					break;
				case Turo3d.Stato.Baza baza:
					if (malamiko.EstasVundiga())
					{
						ludanta.Turo.stato = new Turo3d.Stato.Mortinta(RegenerDuro);
						ludanta.Turo.Hide();
						// farenda: kreu gibletojn
						stato = ludanta with { Vivoj = ludanta.Vivoj - 1 };
						if (ludanta.Vivoj < 0)
						{
							//farenda: GAME OVER
						}
					}
					break;
				case Turo3d.Stato.Mortinta mortinta:
					break;
				case Turo3d.Stato.Vundata vundata:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void DetruiEnergion(Energio energio)
	{
		energio.claimed = true;
		energio.QueueFree();
	}
	private void AkiriEnergion(Energio energio)
	{
		if (!energio.claimed && stato is Stato.Ludanta ludanta)
		{
			ludanta.Turo.energiKvanto++;
		}
		energio.claimed = true;
		energio.QueueFree();
	}
}
