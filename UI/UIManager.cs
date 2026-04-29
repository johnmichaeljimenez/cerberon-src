using Main.Core;

namespace Main.UI;

public static class UIManager
{
	private static readonly Stack<BaseScreen> _stack = new Stack<BaseScreen>();
	public static BaseScreen Current => _stack.Count > 0 ? _stack.Peek() : null;

	private const string LAYOUT = "Assets/UI/layout.tsv";

	private static readonly Dictionary<string, List<UIElement>> elements = new();

	public static void Init()
	{
		elements.Clear();
		foreach (var i in TsvParser.Parse<UIElement>(AssetWatcher.Add(LAYOUT, OnTSVLayoutChanged)))
		{
			if (!elements.ContainsKey(i.Group))
				elements[i.Group] = new();

			elements[i.Group].Add(i);
		}

		SetRoot(ShowScreen<MainMenuScreen>(null));
		Game.Instance.OnStateChanged.Subscribe(state =>
		{
			if (state is MenuState)
			{
				SetRoot(ShowScreen<MainMenuScreen>(null));
			}
			else
			{
				SetRoot(ShowScreen<HUDScreen>(null));
			}

		}); //no need to dispose
	}

	public static T ShowScreen<T>(object context) where T : BaseScreen
	{
		var screen = Activator.CreateInstance<T>();
		if (elements.ContainsKey(screen.UIGroup))
			screen.UpdateElements(elements[screen.UIGroup]);

		screen.OnEnter(context);

		return screen;
	}

	private static void OnTSVLayoutChanged(string content)
	{
		try
		{
			var temp = TsvParser.Parse<UIElement>(content); //do not overwrite layout data if this is faulty
			elements.Clear();

			foreach (var i in temp)
			{
				if (!elements.ContainsKey(i.Group))
					elements[i.Group] = new();

				elements[i.Group].Add(i);
			}

			if (elements.ContainsKey(Current.UIGroup))
				Current.UpdateElements(elements[Current.UIGroup]);
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

	public static void Update(float dt, float udt) //UI stuff will mostly use udt
	{
		Current?.Update(dt, udt);
	}

	public static void Draw()
	{
		Current?.Draw();
	}

	public static void Push(BaseScreen screen)
	{
		Current?.Dispose();

		_stack.Push(screen);
		screen.OnEnter();
	}

	// Pop the current screen off the stack
	public static void Pop()
	{
		if (_stack.Count == 0) return;

		var top = _stack.Pop();
		top.Dispose();

		Current?.OnEnter();
	}

	public static void SetRoot(BaseScreen screen)
	{
		while (_stack.Count > 0) Pop();
		Push(screen);
	}
}