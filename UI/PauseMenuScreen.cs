using Cerberon.Core;
using Cerberon.Gameplay;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class PauseMenuScreen : BaseScreen
{
	public override string UIGroup => "PauseMenu";

	private GameplayState gameplayState = null;

	public PauseMenuScreen(object context) : base(context)
	{
		gameplayState = context as GameplayState;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		InputManager.SetCursorState("pause", CursorType.DefaultSystem);
	}

	public override void Dispose()
	{
		InputManager.RemoveCursorState("pause");
		base.Dispose();
	}

	public override bool OnBack()
	{
		return false;
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-resume":
				gameplayState.PauseGame(false);
				break;
			case "btn-options":
				UIManager.ShowScreen<OptionsScreen>(null, false);
				break;
			case "btn-quit-to-menu":
				FadeHandler.FadeIn(Game.Instance.GoToMenu, true);
				break;
			default:
				break;
		}
	}
}