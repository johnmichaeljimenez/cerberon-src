using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class MainMenuScreen : BaseScreen
{
	public override string UIGroup => "MainMenu";

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