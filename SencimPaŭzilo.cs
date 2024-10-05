using Godot;
using System;
using System.Threading.Tasks;

public partial class SencimPaŭzilo : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TraktiPaŭzo();
	}

	public async Task TraktiPaŭzo()
	{
		while (true)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (Input.IsActionJustReleased("debug_pause"))
			{
				GetTree().Paused = true;
				while (true)
				{
					if (Input.IsActionJustPressed("debug_pause"))
					{
						break;
					}
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}

				while (true)
				{
					if (Input.IsActionJustPressed("debug_pause"))
					{
						break;
					}
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
				float time = Time.GetTicksMsec();
				while (true)
				{
					if (Input.IsActionJustReleased("debug_pause"))
					{
						GetTree().Paused = false;
						var newTime = Time.GetTicksMsec();
						if (newTime - time < 250f)
						{
							await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
							GetTree().Paused = true;
							while (true)
							{
								if (Input.IsActionJustPressed("debug_pause"))
								{
									break;
								}
								await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
							}
							time = Time.GetTicksMsec();
						}
						else
						{
							break;
						}
					}
					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}
			}
		}
	}
}
