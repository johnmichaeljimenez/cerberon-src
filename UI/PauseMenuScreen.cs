using Main.Core;
using Main.Gameplay;
using Main.Helpers;

namespace Main.UI;

public class PauseMenuScreen : BaseScreen
{
	public override string UIGroup => "PauseMenu";

	private GameplayState gameplayState = null;

	public PauseMenuScreen(object context) : base(context)
	{
		gameplayState = (GameplayState)context;
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-resume":
				gameplayState.PauseGame(false);
				break;
			case "btn-quit-to-menu":
				Game.Instance.GoToMenu();
				break;
			default:
				break;
		}
	}
}