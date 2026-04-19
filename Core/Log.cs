namespace Main.Core;

public static class Log
{
	public static string Messages { get; private set; } = "";
	private static bool scrollNow;

	public static void Send(string msg)
	{
		Messages += $"[{Time.CurrentTime:F2}] {msg}\n";
		scrollNow = true;
	}

	public static void DrawImGui()
	{
		ImGui.SetNextWindowSize(new(400, 500), ImGuiCond.FirstUseEver);
		if (ImGui.Begin("Log"))
		{
			ImGui.TextUnformatted(Messages);

			if (scrollNow)
			{
				ImGui.SetScrollHereY(1f);
				scrollNow = false;
			}

			ImGui.End();
		}
	}
}