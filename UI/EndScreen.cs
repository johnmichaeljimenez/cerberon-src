using Main.Core;
using Main.Gameplay;
using Main.Helpers;

namespace Main.UI;

public class EndScreen : BaseScreen
{
	public override string UIGroup => "End";

	private GameplayState gameplayState;
	private bool win;

	public EndScreen(object context) : base(context)
	{
		var ctx = ((GameplayState, bool))context;
		gameplayState = ctx.Item1;
		win = ctx.Item2;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		references["result-text"].Text = win? "Mission Complete" : "Game Over";
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-restart":
				FadeHandler.FadeIn(Game.Instance.RestartGame, true);
				break;
			case "btn-quit-to-menu":
				FadeHandler.FadeIn(Game.Instance.GoToMenu, true);
				break;
			default:
				break;
		}
	}
}