using Cerberon.Core;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class OptionsScreen : BaseScreen
{
	public override string UIGroup => "Options";

	public OptionsScreen(object context) : base(context)
	{
		
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-start":
				UIManager.ShowScreen<LevelSelectScreen>(null, false);
				// FadeHandler.FadeIn(Game.Instance.GoToIngame, true);
				break;
			case "btn-exit":
				FadeHandler.FadeIn(Game.Instance.RequestExit);
				break;
			default:
				break;
		}
	}
}