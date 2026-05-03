using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class LevelSelectScreen : BaseScreen
{
	public override string UIGroup => "LevelSelect";

	private readonly List<string> visibleLevels = new();

	private int page = 0;

	private readonly Dictionary<string, int> itemIndices = new()
	{
		{"btn-level-1", 0},
		{"btn-level-2", 1},
		{"btn-level-3", 2},
		{"btn-level-4", 3},
		{"btn-level-5", 4},
	};

	public LevelSelectScreen(object context) : base(context)
	{

	}

	public override void OnEnter()
	{
		base.OnEnter();

		page = 0;
		UpdateList();
	}

	private void UpdateList()
	{
		visibleLevels.Clear();
		visibleLevels.AddRange(Utils.GetPage(AssetManager.LevelFiles.Keys.ToList(), 5, ref page, out var pageCount));

		for (int i = 0; i < 5; i++)
		{
			var id = $"btn-level-{i + 1}";
			var e = references[id];

			e.Visible = visibleLevels.Count > i;
			if (!e.Visible)
				continue;

			e.Text = AssetManager.LevelFiles[visibleLevels[i]];
		}

		references["btn-level-prev"].Visible = page > 1;
		references["btn-level-next"].Visible = page < pageCount - 1;
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		if (itemIndices.ContainsKey(e.ID))
		{
			FadeHandler.FadeIn(() => Game.Instance.GoToIngame(new()
			{
				LevelFileName = visibleLevels[itemIndices[e.ID]]
			}), true);
			return;
		}

		switch (e.ID)
		{
			case "btn-back":
				UIManager.Back();
				break;
			default:
				break;
		}
	}
}