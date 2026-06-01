using Cerberon.Core;

namespace Cerberon.Helpers;

public static class QuakeFlicker
{
	private static readonly string pattern = "mmnmmommommnonmmonqnmmo"; //from Quake
	private static int maxLevel = 16;
	private const float FrameDuration = 0.1f; //Quake uses 0.1s per frame(10 Hz)

	public static float GetIntensity(int? offset = null)
	{
		int frameIndex = (int)Math.Floor(Time.CurrentTime / FrameDuration) % pattern.Length;
		if (offset.HasValue)
			frameIndex += offset.Value;

		frameIndex %= pattern.Length; //cycler
		if (frameIndex < 0)
			frameIndex += pattern.Length;

		char c = pattern[frameIndex];

		int level = char.ToLowerInvariant(c) - 'a';
		if (level < 0) level = 0;
		if (level > maxLevel) level = maxLevel;

		return level / (float)maxLevel;
	}
}