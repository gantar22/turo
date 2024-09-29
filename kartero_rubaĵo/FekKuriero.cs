using Godot;
using System;
using System.Threading.Tasks;

public partial class FekKuriero : Sprite2D
{
	[Export] private float _moveSpeed = 100f;
	
	private Vector2 _moveDir;

	private SceneTreeTimer _turnTimer;
	
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		_turnTimer = GetTree().CreateTimer(GD.RandRange(3f, 5f));
		_moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,1f).Normalized();
		await TurnRoutine();
	}

	public async Task TurnRoutine()
	{
		while (true)
		{
			await ToSignal(_turnTimer, Timer.SignalName.Timeout);
			_turnTimer = GetTree().CreateTimer(GD.RandRange(3f, 5f));
			_moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,-1f).Normalized();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var pos = GetPosition();
		pos += (float)delta * _moveDir * _moveSpeed;
		pos.X = pos.X % GetViewport().GetVisibleRect().Size.X;
		SetPosition(pos);
	}
}
