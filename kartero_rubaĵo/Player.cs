using Godot;
using System;
using System.Threading.Tasks;

public partial class Player : Node2D
{
	private string lastMoveKeyPressed = "";

	[Export]
	public float moveSpeed = 250f;

	[Export] public float maxBoostMod = 3f;

	public float energio = 1f;
	
	public enum State
	{
		Passive,
		Charge,
		Attack,
	}

	public State state;
	Vector2 _inputDir;

	private float _speedModifier = 1f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var _ = ProcessInput();
		var __ = ProcessAttack();
	}

	private async Task ProcessInput()
	{
		while (true)
		{

			foreach (var key in new[] { "move_down", "move_up", "move_right", "move_left" })
			{
				if (Input.IsActionJustPressed(key))
				{
					lastMoveKeyPressed = key;
				}
			}

			if (lastMoveKeyPressed != "")
			{
				if (!Input.IsActionPressed(lastMoveKeyPressed))
				{
					lastMoveKeyPressed = "";
				}
			}

			switch (lastMoveKeyPressed)
			{
				case "move_down":
					_inputDir = Vector2.Down;
					break;
				case "move_up":
					_inputDir = Vector2.Up;
					break;
				case "move_right":
					_inputDir = Vector2.Right;
					break;
				case "move_left":
					_inputDir = Vector2.Left;
					break;
				default:
					_inputDir = Vector2.Zero;
					break;
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task ProcessAttack()
	{
		while (true)
		{
			switch (state)
			{
				case State.Passive:
					if (Input.IsActionJustPressed("primary"))
					{
						state = State.Charge;
						ProcessCharge();
					}
					break;
				case State.Charge:
					if (Input.IsActionJustReleased("primary"))
					{
						state = State.Attack;
					}
					break;
				case State.Attack:
					break;
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task ProcessCharge()
	{
		var line = (Line2D)GetNode("Line2D");
		line.SetPoints(new Vector2[2]);
		Vector2 attackDir = _inputDir;
		float length = 0f;
		while (state == State.Charge)
		{
			float delta = (float)GetProcessDeltaTime();
			if (energio > 0)
			{
				energio -= delta;
				length += delta * 400f;
			}
			else
			{
				// flash the bar
			}
			line.SetPointPosition(1,attackDir * length);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		if (state == State.Attack)
		{
			ProcessRelease(Position + length * attackDir);
		}
	}

	private async Task ProcessRelease(Vector2 target)
	{
		float alpha = 0f;
		Vector2 start = Position;
		Vector2 end = target;
		var line = (Line2D)GetNode("Line2D");
		while (state == State.Attack && alpha <= 1)
		{
			alpha += (float)GetProcessDeltaTime() * 5f;
			Position = start.Lerp(end, alpha);
			line.SetPointPosition(1,target - Position);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		line.SetPoints(Array.Empty<Vector2>());
		if (alpha >= 1)
		{
			await ToSignal(GetTree().CreateTimer(.05),Timer.SignalName.Timeout);
			state = State.Passive;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (state != State.Charge)
		{
			energio += .5f * (float)delta;
		}
		
		var pos = GetPosition();
		pos += _inputDir * (float)delta * moveSpeed * (state == State.Charge ? .5f : 1f);
		var viewRect = GetViewportRect();
		{
			pos.X = (pos.X + (pos.X < 0 ? viewRect.Size.X : 0f)) % viewRect.Size.X;
			pos.Y = (pos.Y + (pos.Y < 0 ? viewRect.Size.Y : 0f)) % viewRect.Size.Y;
		}
		SetPosition(pos);
	}
}
