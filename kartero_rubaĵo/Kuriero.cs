using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Kuriero : Node2D
{
	[Export] private float _moveSpeed = 100f;

	[Signal]
	public delegate void OnHitPlayerEventHandler();
	
	private Vector2 _moveDir;

	private SceneTreeTimer _turnTimer;

	private Player _target;

	public enum State
	{
		Passive,
		Charge,
		Attack,
	}

	public State state = State.Passive;
	
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		_turnTimer = GetTree().CreateTimer(GD.RandRange(3f, 5f));
		_moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,1f).Normalized();
		// bizara magiko: sen rulilo, la asynka funkcia ruliĝas
	}

	public async void Init(Player _player)
	{
		_target = _player;
		var _ = TurnRoutine();
	}

	private async Task TurnRoutine()
	{
		while (true)
		{
			// await ToSignal(_turnTimer, Timer.SignalName.Timeout);
			// _turnTimer = GetTree().CreateTimer(GD.RandRange(2f, 5f));
			// _moveDir = new Vector2(GD.Randf() > .5f ? 1f : -1f,1f).Normalized();
			var attackAngle = (_target.Position - Position).Normalized();
			var dirs = new[]
			{
				new Vector2( 1, 1),
				new Vector2( 1,-1),
				new Vector2(-1, 1),
				new Vector2(-1,-1)
			};
			var orderedDirs = dirs.OrderBy(_ => Mathf.Abs(_.AngleTo(attackAngle))).ToArray();
			var desiredDir = orderedDirs.Skip(1).FirstOrDefault().Normalized();
			var closestDir = orderedDirs.FirstOrDefault().Normalized();
			if (Mathf.Abs(closestDir.AngleTo(attackAngle)) < .1f)
			{
				await ToSignal(GetTree().CreateTimer(.2), Timer.SignalName.Timeout);
				// charge up an attack
				state = State.Charge;
				_moveDir = new Vector2();
				var line = (Line2D)GetNode("Line2D");
				line.Points = new Vector2[2];
				line.Points[0] = new Vector2();
				line.Points[1] = new Vector2();
				float length = 0f;
				float limit = Mathf.Max(attackAngle.Dot(_target.Position - Position), 200f);
				while (length < limit && GetViewportRect().HasPoint(Position + attackAngle * length))
				{
					line.SetPointPosition(1,attackAngle * length);
					length += (float)GetProcessDeltaTime() * 475f;
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
					limit = Mathf.Max(attackAngle.Dot(_target.Position - Position), 200f);
				}

				await ToSignal(GetTree().CreateTimer(.125), Timer.SignalName.Timeout);
				
				// release
				state = State.Attack;
				var alpha = 0f;
				Vector2 start = Position;
				Vector2 end = Position + attackAngle * length;
				while (alpha <= 1f)
				{
					alpha += (float)GetProcessDeltaTime() * 4.5f;
					Position = start.Lerp(end, alpha);
					line.SetPointPosition(1,end - Position);
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
				
				line.Points = Array.Empty<Vector2>();
				// end
				state = State.Passive;
				await ToSignal(GetTree().CreateTimer(.75), Timer.SignalName.Timeout);
			}
			_moveDir = desiredDir;
			if (!GetViewportRect().HasPoint(Position + _moveDir * 100f))
			{
				_moveDir = dirs.MinBy(_=>Mathf.Abs(_.AngleTo(GetViewportRect().GetCenter() - Position)));
				await ToSignal(GetTree().CreateTimer(.5), Timer.SignalName.Timeout);
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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
		var speed = _moveSpeed * (state == State.Attack ? 20 : 1);
		poz += (float)delta * _moveDir * _moveSpeed;
		SetPosition(poz);
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body.Owner is Player player)
		{
			EmitSignal(SignalName.OnHitPlayer);
		}
	}
}
