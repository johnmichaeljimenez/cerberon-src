using Main.Core;

namespace Main.UI;

public static class UIManager
{
	private const string LAYOUT = "Assets/UI/layout.tsv";

	private static readonly List<UIElement> elements = new();

	public static void Init()
	{
		elements.Clear();
		elements.AddRange(TsvParser.Parse<UIElement>(AssetWatcher.Add(LAYOUT, OnTSVLayoutChanged)));
	}

	private static void OnTSVLayoutChanged(string content)
	{
		try
		{
			var temp = TsvParser.Parse<UIElement>(content); //do not overwrite layout data if this is faulty
			elements.Clear();
			elements.AddRange(temp);
		}
		catch (Exception ex)
		{
			Log.Send($"UI TSV error: {ex.Message}");
		}
	}

	public static void Dispose()
	{
		AssetWatcher.Remove(LAYOUT);
	}

	public static void Draw()
	{
		foreach (var i in elements)
		{
			if (!i.Visible)
				continue;

			i.Draw();
		}
	}
}