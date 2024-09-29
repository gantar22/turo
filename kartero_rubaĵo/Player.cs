using Godot;
using System;

public partial class Player : Node2D
{
	private string lastMoveKeyPressed = "";

	[Export]
	public float moveSpeed = 100f;

	[Export] public float maxBoostMod = 3f;

	private float _speedModifier = 1f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach(var key in new [] {"move_down","move_up","move_right","move_left"})
		{
			if (Input.IsActionJustPressed(key))
			{
				lastMoveKeyPressed = key;
			}
		}
		Vector2 inputDir;
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
				inputDir = Vector2.Down;
				break;
			case "move_up":
				inputDir = Vector2.Up;
				break;
			case "move_right":
				inputDir = Vector2.Right;
				break;
			case "move_left":
				inputDir = Vector2.Left;
				break;
			default:
				inputDir = Vector2.Zero;
				break;
		}

		var pos = GetPosition();
		pos += inputDir * (float)delta * moveSpeed;
		var viewRect = GetViewportRect();
		{
			pos.X = (pos.X + (pos.X < 0 ? viewRect.Size.X : 0f)) % viewRect.Size.X;
			pos.Y = (pos.Y + (pos.Y < 0 ? viewRect.Size.Y : 0f)) % viewRect.Size.Y;
		}
		SetPosition(pos);
	}
}
