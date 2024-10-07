using Godot;

namespace Turo.Ludo.Utilaĵoj;

public static class Vektoroj
{
	public static Vector2I RoundToInt(this Vector2 v)
	{
		return new Vector2I(Mathf.RoundToInt(v.X), Mathf.RoundToInt(v.Y));
	}

	public static Vector2 AsFloat(this Vector2I v)
	{
		return new Vector2(v.X, v.Y);
	}

	public static Vector2I FloorToInt(this Vector2 v)
	{
		return new Vector2I(Mathf.FloorToInt(v.X), Mathf.FloorToInt(v.Y));
	}
}