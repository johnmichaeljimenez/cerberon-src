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
		percentage = Raymath.Clamp01(percentage);
		
		var roll = rng.NextDouble();
		return roll < percentage;
	}

	public static Vector2 Position(float range = 1.0f)
	{
		var pos = new Vector2(Range(-1f, 1f), Range(-1f, 1f));
		if (pos.Length() > 1.0f)
			pos = Raymath.Vector2Normalize(pos);

		return pos * range;
	}
};