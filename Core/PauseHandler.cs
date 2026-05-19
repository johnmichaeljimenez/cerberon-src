using Main.Helpers;
using Tween;

namespace Main.Core;

public static class PauseHandler
{
	public static bool IsPaused { get; private set; }
	private static float hitstop;
	private static readonly RefCount pauseList = new() //used not only for pause menu but also for ex. dialogues, inventory, or even millisecond hitstops during combat
	{
		OnChanged = UpdatePause
	};

	public static void Pause(string id)
	{
		pauseList.Add(id);
	}

	public static void Unpause(string id)
	{
		pauseList.Remove(id);
	}

	public static void Clear()
	{
		pauseList.Clear();
	}

	private static void UpdatePause()
	{
		IsPaused = pauseList.IsActive;
	}

	public static void ApplyHitstop(float duration = 0.1f)
	{
		var tween = new Tween<float>(() => hitstop, p => hitstop = p, 1, duration, 0, "hitstop", true);
		tween.OnComplete(() => Unpause("hitstop"));
		TweenManager.Add(tween);

		Pause("hitstop");
	}
}