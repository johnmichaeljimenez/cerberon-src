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

	public override void Draw()
	{
		base.Draw();

		foreach (var item in elements)
		{
			var hovered = hoveredElement == item;
			var r = item.Rect;
			r.Expand(hovered ? 2f : 1);

			Raylib.DrawRectangleRec(r, hovered ? Color.Green : Color.DarkBlue);
			Raylib.DrawRectangleLines((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height, Color.White);
		}
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