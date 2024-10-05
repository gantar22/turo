using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ludanto;

public class MovEnigo
{
	public abstract class Enigo
	{
		public abstract string ŝlosilo { get; }
		public abstract Vector2 Direkto { get; }

		public class Sube : Enigo
		{
			public override string ŝlosilo => "move_down";
			public override Vector2 Direkto => Vector2.Down;
		}
		public class Supre : Enigo
		{
			public override string ŝlosilo => "move_up";
			public override Vector2 Direkto => Vector2.Up;
		}
		public class Dekstre : Enigo
		{
			public override string ŝlosilo => "move_right";
			public override Vector2 Direkto => Vector2.Right;
		}
		public class Live : Enigo
		{
			public override string ŝlosilo => "move_left";
			public override Vector2 Direkto => Vector2.Left;
		}

		public static readonly Enigo[] enigoj = new Enigo[] { new Sube(), new Supre(), new Dekstre(), new Live() };
	}
	
	private List<Enigo> premataEnigoj = new List<Enigo>();

	public Enigo AkiriEnigo()
	{
		var enigontoj = new List<Enigo>();
		foreach (var enigo in premataEnigoj)
		{
			if (Input.IsActionPressed(enigo.ŝlosilo) && !Input.IsActionJustPressed(enigo.ŝlosilo))
			{
				enigontoj.Add(enigo);
			}
		}
		foreach (var enigo in Enigo.enigoj)
		{
			if (Input.IsActionJustPressed(enigo.ŝlosilo))
			{
				enigontoj.Add(enigo);
			}
		}

		premataEnigoj = enigontoj;
		return premataEnigoj.LastOrDefault();
	}
}