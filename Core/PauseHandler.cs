namespace Main.Core;

public static class PauseHandler
{
	public static bool IsPaused { get; private set; }
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

	private static void UpdatePause()
	{
		IsPaused = pauseList.Count > 0;
	}
}