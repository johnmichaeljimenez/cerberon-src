namespace Main.Helpers;

public static class RNG
{
	private static readonly Random rng = new Random();

	public static float Range(float min, float max)
	{
		if (min > max) throw new ArgumentException("min must be ≤ max");
		var sample = rng.NextDouble();
		return (float)(min + (sample * (max - min)));
	}

	public static int Range(int min, int max)
	{
		if (min > max) throw new ArgumentException("min must be ≤ max");
		return rng.Next(min, max);
	}

	public static bool Chance(float percentage)
	{
		if (percentage <= 0f) return false;
		if (percentage >= 100f) return true;
		
		var roll = rng.NextDouble() * 100.0;
		return roll < percentage;
	}
};