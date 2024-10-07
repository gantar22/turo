namespace Turo.Ludo.Utilaĵoj;

public static class Easings
{
	public static float EaseInOut(float a)
	{
		float a3 = a * a * a;
		float polynomial = a * (6f * a - 15f) + 10f; 
		return a3 * polynomial;
	}
}