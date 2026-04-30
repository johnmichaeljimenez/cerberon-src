using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class MainMenuScreen : BaseScreen
{
	public override string UIGroup => "MainMenu";

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-start":
				Game.Instance.GoToIngame();
				break;
			case "btn-exit":
				Game.Instance.RequestExit();
				break;
			default:
				break;
		}
	}
}