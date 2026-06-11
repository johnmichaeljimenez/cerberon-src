using Cerberon.Core;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class OptionsScreen : BaseScreen
{
	public override string UIGroup => "Options";

	public OptionsScreen(object context) : base(context)
	{

	}

	public override void UpdateElements(List<UIElement> elements)
	{
		base.UpdateElements(elements);
		UpdateState();
	}

	private void UpdateState()
	{
		references["btn-sfx"].Text = $"Sound Effects: {(AudioHandler.SoundEnabled ? "ON" : "OFF")}";
		references["btn-music"].Text = $"Music: {(AudioHandler.MusicEnabled ? "ON" : "OFF")}";
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-sfx":
				AudioHandler.SoundEnabled = !AudioHandler.SoundEnabled;
				UpdateState();
				break;
			case "btn-music":
				AudioHandler.MusicEnabled = !AudioHandler.MusicEnabled;
				UpdateState();
				break;
			case "btn-safe":
				break;
		}
	}
}