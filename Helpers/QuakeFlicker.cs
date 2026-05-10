using Main.Core;

namespace Main.Helpers;

public static class QuakeFlicker
{
	private static readonly string pattern = "mmnmmommommnonmmonqnmmo"; //from Quake
	private const float FrameDuration = 0.1f; //Quake uses 0.1s per frame(10 Hz)

	public static float GetIntensity()
	{
		int frameIndex = (int)Math.Floor(Time.CurrentTime / FrameDuration) % pattern.Length;
		char c = pattern[frameIndex];

		int level = char.ToLowerInvariant(c) - 'a';
		if (level < 0) level = 0;
		if (level > 25) level = 25;

		return level / 25.0f;
	}
}