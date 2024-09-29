using Godot;
using System;
using System.Threading.Tasks;

public partial class Kuriero : Node2D
{
	[Export] private float _moveSpeed = 100f;
	
	private Vector2 _moveDir;

	private SceneTreeTimer _turnTimer;
	
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		Visible = false;
		await ToSignal(GetTree().CreateTimer(GD.RandRange(3f, 12f)),Timer.SignalName.Timeout);
		Visible = true;
		_turnTimer = GetTree().CreateTimer(GD.RandRange(3f, 5f));
		_moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,1f).Normalized();
		TurnRoutine();
	}

	private async Task TurnRoutine()
	{
		while (true)
		{
			await ToSignal(_turnTimer, Timer.SignalName.Timeout);
			_turnTimer = GetTree().CreateTimer(GD.RandRange(2f, 5f));
			_moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,1f).Normalized();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var poz = GetPosition();
		// resalti ĉe muroj
		{
			var prov_poz = poz + (float)delta * _moveDir * _moveSpeed;
			var viewRect = GetViewportRect();
			if (prov_poz.X < 0 || prov_poz.X > viewRect.Size.X)
			{
				_moveDir.X *= -1;
			}

			if (prov_poz.Y < 0 || prov_poz.Y > viewRect.Size.Y)
			{
				_moveDir.Y *= -1;
			}
		}
		poz += (float)delta * _moveDir * _moveSpeed;
		SetPosition(poz);
	}
}
