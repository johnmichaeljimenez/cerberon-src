using Tween;

namespace Main.Core;

public static class PauseHandler
{
	public static bool IsPaused { get; private set; }
	private static float hitstop;
	private static readonly HashSet<string> pauseList = new(); //used not only for pause menu but also for ex. dialogues, inventory, or even millisecond hitstops during combat

	public static void Pause(string id)
	{
		if (pauseList.Add(id))
			UpdatePause();
	}

	public static void Unpause(string id)
	{
		if (pauseList.Remove(id))
			UpdatePause();
	}

	public static void Clear()
	{
		hitstop = 0;
		pauseList.Clear();
		UpdatePause();
	}

	private static void UpdatePause()
	{
		IsPaused = pauseList.Count > 0;
	}

	public static void ApplyHitstop(float duration = 0.1f)
	{
		var tween = new Tween<float>(() => hitstop, p => hitstop = p, 1, duration, 0, "hitstop", true);
		tween.OnComplete(() => Unpause("hitstop"));
		TweenManager.Add(tween);

		Pause("hitstop");
	}
}