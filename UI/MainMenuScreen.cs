using Cerberon.Core;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class MainMenuScreen : BaseScreen
{
	public override string UIGroup => "MainMenu";

	private Sprite[] frames;
	private int frameIndex;
	private readonly EMA flickerEMA = new(0.1f);

	public MainMenuScreen(object context) : base(context)
	{
		frames = [
			AssetManager.GetSprite("ui/logo"),
			AssetManager.GetSprite("ui/logo-glow"),
			AssetManager.GetSprite("ui/logo-glow-2"),
		];

		frameIndex = RNG.Range(0, frames.Length);
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
			flickerEMA.AddSample(flicker);
			if (flickerEMA.Current >= 0.85f)
			{
				frameIndex++;
				if (frameIndex >= frames.Length)
				{
					frameIndex = 0;
					frames.Shuffle();
				}
			}

			var sprite = frames[frameIndex];
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