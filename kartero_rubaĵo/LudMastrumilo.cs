using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class LudMastrumilo : Node2D
{
	public abstract record class Stato
	{
		public record ĈefmenuStato
		{
			public Ĉefmenuo Ĉefmenuo;
		}

		public record Ludo
		{
			private HashSet<Kuriero2D> _vivajKurieroj = new HashSet<Kuriero2D>();
			
			// farenda: movu sanon al la ludanto
			public double _playerHealth;
			private Ludanto.Ludanto _turo;
		}
	}
	
	[Export]
	public PackedScene TuroScene { get; set; }
	
	[Export]
	public PackedScene KurieroScene { get; set; }
	
	[Export] private PackedScene ĈefmenuSceno {get; set; }

	[Export] private Camera2D _kamerao;
	
	private double _playerHealth = 100;

	private Ĉefmenuo _ĉefmenuo;

	private HashSet<Kuriero2D> _vivajKurieroj = new HashSet<Kuriero2D>();

	private Ludanto.Ludanto _turo = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_ĉefmenuo = (Ĉefmenuo)ĈefmenuSceno.Instantiate();
		_ĉefmenuo.Reparent(_kamerao,false);
		_ĉefmenuo.EkButono.Pressed += () =>
		{
			_ĉefmenuo.EkButono.Hide();
			
			_turo = (Ludanto.Ludanto)TuroScene.Instantiate();
			_turo.Position = GetViewportRect().Position + .5f * GetViewportRect().Size;
			AddChild(_turo);
			var _ = _SpawnRoutine();
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ProgressBar Sano = (ProgressBar)GetNode("CanvasLayer/Sano");
		Sano.Value = _playerHealth;
		ProgressBar Energio = (ProgressBar)GetNode("CanvasLayer/Energio");
		Energio.Value = _turo.energio * 100;
	}

	private async Task _SpawnRoutine()
	{
		await ToSignal(GetTree().CreateTimer(2.5), Timer.SignalName.Timeout);
		int nivelo = 1;
		while (true)
		{
			List<Task> tasks = new List<Task>();
			Rect2 viewport = GetViewportRect();
			for (int i = 0; i < nivelo; i++)
			{
				Rect2 zone = new Rect2();
				{
					zone.Position = new Vector2((float)GD.RandRange(0,viewport.Size.X),(float)GD.RandRange(0,viewport.Size.Y));
					zone.End = new Vector2((float)GD.RandRange(zone.Position.X,viewport.Size.X),(float)GD.RandRange(zone.Position.Y,viewport.Size.Y));
				};
				tasks.Add(_SpawnOneKuriero(zone));
			}
			while (tasks.Any(_=>!_.IsCompleted))
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			nivelo++;
			await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);
		}
	}

	private async Task _SpawnOneKuriero(Rect2 Zone)
	{
		while (Zone.HasPoint(_turo.Position))
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		
		var malamiko = (Kuriero2D)KurieroScene.Instantiate();
		AddChild(malamiko);
		_vivajKurieroj.Add(malamiko);
		malamiko.Position = Zone.Position + .5f * Zone.Size;
		malamiko.Init(_turo);
		bool done = false;
		malamiko.OnHitPlayer += () =>
		{
			if (malamiko.state == Kuriero2D.State.Attack)
			{
				_playerHealth -= 10;
			} else if (_turo.stato is not Ludanto.Ludanto.Stato.Atakanta)
			{
				_playerHealth -= 5;
			}
			_vivajKurieroj.Remove(malamiko);
			malamiko.QueueFree();
			done = true;
		};
		await ToSignal(malamiko, Node.SignalName.TreeExiting);
	}
}
