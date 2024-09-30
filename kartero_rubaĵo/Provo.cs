using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Provo : Node2D
{
	[Export]
	public PackedScene TuroScene { get; set; }
	
	[Export]
	public PackedScene KurieroScene { get; set; }
	
	private double _playerHealth = 100;

	private HashSet<Kuriero> _vivajKurieroj = new HashSet<Kuriero>();

	private Player _turo = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_turo = (Player)TuroScene.Instantiate();
		_turo.Position = GetViewportRect().Position + .5f * GetViewportRect().Size;
		AddChild(_turo);
		var _ = _SpawnRoutine();
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
		
		var malamiko = (Kuriero)KurieroScene.Instantiate();
		AddChild(malamiko);
		_vivajKurieroj.Add(malamiko);
		malamiko.Position = Zone.Position + .5f * Zone.Size;
		malamiko.Init(_turo);
		bool done = false;
		malamiko.OnHitPlayer += () =>
		{
			if (malamiko.state == Kuriero.State.Attack)
			{
				_playerHealth -= 10;
			} else if (_turo.state != Player.State.Attack)
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
