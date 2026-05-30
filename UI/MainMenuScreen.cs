using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class MainMenuScreen : BaseScreen
{
	public override string UIGroup => "MainMenu";

	private Sprite logoDefault, logoGlow;

	public MainMenuScreen(object context) : base(context)
	{
		logoDefault = AssetManager.GetSprite("ui/logo");
		logoGlow = AssetManager.GetSprite("ui/logo-glow");
	}

	public override void OnBack()
	{

	}

	public override void UpdateElements(List<UIElement> elements)
	{
		base.UpdateElements(elements);

		var title = references["title"];
		title.CustomDrawing = (e) =>
		{
			var flicker = QuakeFlicker.GetIntensity();
			var sprite = flicker >= 0.8f ? logoGlow : logoDefault;
			var r = e.GetAspectRatioRectangle(new(sprite.Width, sprite.Height));

			Raylib.DrawTexturePro(sprite.Texture,
				new Rectangle(0, 0, sprite.Width, sprite.Height),
				r,
				Vector2.Zero, 0, Color.White);
		};
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